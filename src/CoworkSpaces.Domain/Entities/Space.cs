using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Domain.Entities;

public class Space
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public SpaceStatus Status { get; set; }
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
