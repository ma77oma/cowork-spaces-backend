using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommand : IRequest<ReservationResponse>
{
    public Guid SpaceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
