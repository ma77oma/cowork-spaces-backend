using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Application.DTOs.Reservations;

public class CancelReservationResponse
{
    public Guid ReservationId { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal RefundAmount { get; set; }
    public DateTime CancelledAt { get; set; }
}
