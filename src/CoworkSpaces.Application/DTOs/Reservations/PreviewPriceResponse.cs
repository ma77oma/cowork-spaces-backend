namespace CoworkSpaces.Application.DTOs.Reservations;

public class PreviewPriceResponse
{
    public Guid SpaceId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal DurationHours { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public decimal FinalPrice { get; set; }
}
