using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Application.DTOs.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public decimal DurationHours { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal? RefundAmount { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
