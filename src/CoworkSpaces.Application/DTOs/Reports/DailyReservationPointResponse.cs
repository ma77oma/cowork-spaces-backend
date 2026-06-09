namespace CoworkSpaces.Application.DTOs.Reports;

public class DailyReservationPointResponse
{
    public DateOnly Date { get; set; }
    public int TotalReservations { get; set; }
}
