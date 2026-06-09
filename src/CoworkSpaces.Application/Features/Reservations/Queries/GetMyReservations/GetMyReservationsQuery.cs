using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQuery : IRequest<IReadOnlyCollection<ReservationResponse>>
{
}
