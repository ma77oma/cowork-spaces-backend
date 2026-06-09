namespace CoworkSpaces.Application.DTOs.Reports;

public class DailyIncomePointResponse
{
    public DateOnly Date { get; set; }
    public decimal TotalIncome { get; set; }
}
