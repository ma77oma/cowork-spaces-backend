using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Commands.ConfirmReservation;

public class ConfirmReservationCommand : IRequest<ReservationResponse>
{
    public Guid ReservationId { get; set; }
}
