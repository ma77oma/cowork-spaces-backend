using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public Space? Space { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string? CancelledByUserId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
