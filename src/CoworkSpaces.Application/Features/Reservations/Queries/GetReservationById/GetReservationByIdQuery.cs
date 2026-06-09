using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Queries.GetReservationById;

public class GetReservationByIdQuery : IRequest<ReservationResponse>
{
    public Guid Id { get; set; }
}
