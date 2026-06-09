namespace CoworkSpaces.Application.DTOs.Reports;

public class ReportResponse
{
    public decimal TotalIncome { get; set; }
    public int TotalReservations { get; set; }
    public int TotalSpacesAnalyzed { get; set; }
    public string MostDemandedHour { get; set; } = string.Empty;
    public List<SpaceReportResponse> Spaces { get; set; } = new();
    public List<DailyIncomePointResponse> IncomeByDay { get; set; } = new();
    public List<DailyReservationPointResponse> ReservationsByDay { get; set; } = new();
    public List<HourDemandPointResponse> DemandByHour { get; set; } = new();
    public List<IncomeBySpacePointResponse> IncomeBySpace { get; set; } = new();
    public List<OccupancyBySpacePointResponse> OccupancyBySpace { get; set; } = new();
}
