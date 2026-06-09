using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CancelReservation;

public class CancelReservationCommand : IRequest<CancelReservationResponse>
{
    public Guid ReservationId { get; set; }
}
