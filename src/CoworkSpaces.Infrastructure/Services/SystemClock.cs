using CoworkSpaces.Application.Common.Interfaces;

namespace CoworkSpaces.Infrastructure.Services;

public class SystemClock : ISystemClock
{
    public DateTime Now => DateTime.Now;
}
