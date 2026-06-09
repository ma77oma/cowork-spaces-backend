namespace CoworkSpaces.Application.DTOs.Reports;

public class IncomeBySpacePointResponse
{
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
}
