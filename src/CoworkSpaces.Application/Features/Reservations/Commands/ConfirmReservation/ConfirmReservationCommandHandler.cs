using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.Mappings;
using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Commands.ConfirmReservation;

public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, ReservationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly PricingService _pricingService;

    public ConfirmReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        PricingService pricingService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pricingService = pricingService;
    }

    public async Task<ReservationResponse> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new BusinessException("Solo un administrador puede confirmar reservas.");
        }

        var reservation = await _context.Reservations
            .Include(item => item.Space)
            .SingleOrDefaultAsync(item => item.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reserva no encontrada.");
        }

        ReservationBusinessValidator.ValidateConfirmation(reservation);

        reservation.Status = ReservationStatus.Confirmed;

        await _context.SaveChangesAsync(cancellationToken);

        return reservation.ToResponse(_pricingService);
    }
}
