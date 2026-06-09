namespace CoworkSpaces.Application.DTOs.Reports;

public class OccupancyBySpacePointResponse
{
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
}
