using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CancelReservation;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, CancelReservationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemClock _clock;
    private readonly CancellationPolicyService _cancellationPolicyService;

    public CancelReservationCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ISystemClock clock,
        CancellationPolicyService cancellationPolicyService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _clock = clock;
        _cancellationPolicyService = cancellationPolicyService;
    }

    public async Task<CancelReservationResponse> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .SingleOrDefaultAsync(reservation => reservation.Id == request.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reserva no encontrada.");
        }

        if (reservation.CreatedByUserId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin"))
        {
            throw new BusinessException("No tiene permisos para cancelar esta reserva.");
        }

        ReservationBusinessValidator.ValidateCancellation(reservation);

        var cancelledAt = _clock.Now;
        var refundAmount = _cancellationPolicyService.CalculateRefundAmount(
            reservation.FinalPrice,
            reservation.StartAt,
            cancelledAt,
            reservation.Status);

        reservation.Status = ReservationStatus.Cancelled;
        reservation.CancelledAt = cancelledAt;
        reservation.CancelledByUserId = _currentUserService.UserId;
        reservation.RefundAmount = refundAmount;

        await _context.SaveChangesAsync(cancellationToken);

        return new CancelReservationResponse
        {
            ReservationId = reservation.Id,
            Status = reservation.Status,
            RefundAmount = refundAmount,
            CancelledAt = cancelledAt
        };
    }
}
