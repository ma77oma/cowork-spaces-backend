namespace CoworkSpaces.Application.DTOs.Reports;

public class SpaceReportResponse
{
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
    public decimal TotalIncome { get; set; }
    public int TotalReservations { get; set; }
}
