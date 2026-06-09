using CoworkSpaces.Application.DTOs.Reservations;
using MediatR;

namespace CoworkSpaces.Application.Features.Reservations.Commands.PreviewReservationPrice;

public class PreviewReservationPriceCommand : IRequest<PreviewPriceResponse>
{
    public Guid SpaceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
