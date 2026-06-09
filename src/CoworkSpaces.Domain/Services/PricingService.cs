namespace CoworkSpaces.Domain.Services;

public class PricingService
{
    private static readonly TimeOnly MorningPeakStart = new(9, 0);
    private static readonly TimeOnly MorningPeakEnd = new(11, 0);
    private static readonly TimeOnly EveningPeakStart = new(17, 0);
    private static readonly TimeOnly EveningPeakEnd = new(19, 0);

    public decimal CalculateFinalPrice(decimal baseHourlyRate, DateTime startAt, DateTime endAt, DateTime createdAt)
    {
        var durationHours = (decimal)(endAt - startAt).TotalMinutes / 60m;
        var finalPrice = baseHourlyRate * durationHours;

        if (OverlapsPeakHours(startAt, endAt))
        {
            finalPrice *= 1.25m;
        }

        if (IsWeekend(startAt))
        {
            finalPrice *= 1.15m;
        }

        if (durationHours >= 4m)
        {
            finalPrice *= 0.90m;
        }

        if ((startAt - createdAt).TotalDays >= 7)
        {
            finalPrice *= 0.95m;
        }

        return decimal.Round(finalPrice, 2, MidpointRounding.AwayFromZero);
    }

    public decimal CalculateDurationHours(DateTime startAt, DateTime endAt)
    {
        return decimal.Round((decimal)(endAt - startAt).TotalMinutes / 60m, 2, MidpointRounding.AwayFromZero);
    }

    private static bool OverlapsPeakHours(DateTime startAt, DateTime endAt)
    {
        var startTime = TimeOnly.FromDateTime(startAt);
        var endTime = TimeOnly.FromDateTime(endAt);

        return Overlaps(startTime, endTime, MorningPeakStart, MorningPeakEnd)
            || Overlaps(startTime, endTime, EveningPeakStart, EveningPeakEnd);
    }

    private static bool Overlaps(TimeOnly start, TimeOnly end, TimeOnly rangeStart, TimeOnly rangeEnd)
    {
        return start < rangeEnd && rangeStart < end;
    }

    private static bool IsWeekend(DateTime dateTime)
    {
        return dateTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}
