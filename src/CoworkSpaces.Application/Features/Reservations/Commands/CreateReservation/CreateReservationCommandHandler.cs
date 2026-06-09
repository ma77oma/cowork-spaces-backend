using System.Data;
using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.Mappings;
using CoworkSpaces.Domain.Entities;
using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemClock _clock;
    private readonly PricingService _pricingService;

    public CreateReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ISystemClock clock,
        PricingService pricingService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _clock = clock;
        _pricingService = pricingService;
    }

    public async Task<ReservationResponse> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var createdAt = _clock.Now;
        var currentUserId = _currentUserService.UserId
            ?? throw new BusinessException("Debe iniciar sesión para crear una reserva.");
        var dbContext = _context as DbContext
            ?? throw new InvalidOperationException("El contexto de aplicación debe derivar de DbContext para soportar concurrencia transaccional SQL Server.");

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var space = await dbContext.Set<Space>()
            .FromSqlInterpolated($"SELECT * FROM Spaces WITH (UPDLOCK, HOLDLOCK) WHERE Id = {request.SpaceId}")
            .SingleOrDefaultAsync(cancellationToken);

        if (space is null)
        {
            throw new NotFoundException("Espacio no encontrado.");
        }

        ReservationBusinessValidator.ValidateReservationWindow(space, request.StartAt, request.EndAt, createdAt);

        var hasOverlap = await _context.Reservations.AnyAsync(
            reservation => reservation.SpaceId == request.SpaceId
                && (reservation.Status == ReservationStatus.Pending || reservation.Status == ReservationStatus.Confirmed)
                && reservation.StartAt < request.EndAt
                && request.StartAt < reservation.EndAt,
            cancellationToken);

        ReservationBusinessValidator.EnsureNoOverlap(hasOverlap);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SpaceId = request.SpaceId,
            Space = space,
            CreatedByUserId = currentUserId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Status = ReservationStatus.Pending,
            FinalPrice = _pricingService.CalculateFinalPrice(space.BaseHourlyRate, request.StartAt, request.EndAt, createdAt),
            CreatedAt = createdAt
        };

        await _context.Reservations.AddAsync(reservation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return reservation.ToResponse(_pricingService);
    }
}
