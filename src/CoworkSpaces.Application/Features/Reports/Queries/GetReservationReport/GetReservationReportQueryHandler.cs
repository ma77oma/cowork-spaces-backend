using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Reports;
using CoworkSpaces.Domain.Entities;
using CoworkSpaces.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reports.Queries.GetReservationReport;

public class GetReservationReportQueryHandler : IRequestHandler<GetReservationReportQuery, ReportResponse>
{
    private readonly IApplicationDbContext _context;

    public GetReservationReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportResponse> Handle(GetReservationReportQuery request, CancellationToken cancellationToken)
    {
        var from = request.From;
        var toExclusive = request.To.TimeOfDay == TimeSpan.Zero ? request.To.Date.AddDays(1) : request.To;

        ReservationBusinessValidator.ValidateReportRange(from, toExclusive);

        var spaces = await _context.Spaces
            .AsNoTracking()
            .OrderBy(space => space.Name)
            .ToListAsync(cancellationToken);

        var reservations = await _context.Reservations
            .AsNoTracking()
            .Include(item => item.Space)
            .Where(reservation => (reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.Completed)
                && reservation.StartAt < toExclusive
                && from < reservation.EndAt)
            .ToListAsync(cancellationToken);

        var spaceReports = spaces.Select(space => BuildSpaceReport(space, reservations, from, toExclusive)).ToList();

        return new ReportResponse
        {
            TotalIncome = decimal.Round(reservations.Sum(GetNetIncome), 2, MidpointRounding.AwayFromZero),
            TotalReservations = reservations.Count,
            TotalSpacesAnalyzed = spaces.Count,
            MostDemandedHour = GetMostDemandedHour(reservations),
            Spaces = spaceReports,
            IncomeByDay = BuildIncomeByDaySeries(reservations, from, toExclusive),
            ReservationsByDay = BuildReservationsByDaySeries(reservations, from, toExclusive),
            DemandByHour = BuildDemandByHourSeries(reservations),
            IncomeBySpace = spaceReports.Select(space => new IncomeBySpacePointResponse
            {
                SpaceId = space.SpaceId,
                SpaceName = space.SpaceName,
                TotalIncome = space.TotalIncome
            }).ToList(),
            OccupancyBySpace = spaceReports.Select(space => new OccupancyBySpacePointResponse
            {
                SpaceId = space.SpaceId,
                SpaceName = space.SpaceName,
                OccupancyRate = space.OccupancyRate
            }).ToList()
        };
    }

    private static List<DailyIncomePointResponse> BuildIncomeByDaySeries(IReadOnlyCollection<Reservation> reservations, DateTime from, DateTime toExclusive)
    {
        var incomeByDay = reservations
            .GroupBy(reservation => DateOnly.FromDateTime(reservation.StartAt.Date))
            .ToDictionary(
                group => group.Key,
                group => decimal.Round(group.Sum(GetNetIncome), 2, MidpointRounding.AwayFromZero));

        return EnumerateDays(from, toExclusive)
            .Select(date => new DailyIncomePointResponse
            {
                Date = date,
                TotalIncome = incomeByDay.GetValueOrDefault(date)
            })
            .ToList();
    }

    private static List<DailyReservationPointResponse> BuildReservationsByDaySeries(IReadOnlyCollection<Reservation> reservations, DateTime from, DateTime toExclusive)
    {
        var reservationsByDay = reservations
            .GroupBy(reservation => DateOnly.FromDateTime(reservation.StartAt.Date))
            .ToDictionary(group => group.Key, group => group.Count());

        return EnumerateDays(from, toExclusive)
            .Select(date => new DailyReservationPointResponse
            {
                Date = date,
                TotalReservations = reservationsByDay.GetValueOrDefault(date)
            })
            .ToList();
    }

    private static List<HourDemandPointResponse> BuildDemandByHourSeries(IReadOnlyCollection<Reservation> reservations)
    {
        return reservations
            .GroupBy(reservation => reservation.StartAt.Hour)
            .OrderBy(group => group.Key)
            .Select(group => new HourDemandPointResponse
            {
                Hour = $"{group.Key:00}:00",
                TotalReservations = group.Count()
            })
            .ToList();
    }

    private static SpaceReportResponse BuildSpaceReport(Space space, IReadOnlyCollection<Reservation> reservations, DateTime from, DateTime toExclusive)
    {
        var spaceReservations = reservations.Where(item => item.SpaceId == space.Id).ToList();
        var reservedHours = spaceReservations.Sum(GetOverlappedHours);
        var totalAvailableHours = CalculateAvailableHours(space, from, toExclusive);
        var occupancyRate = totalAvailableHours == 0
            ? 0m
            : decimal.Round((decimal)(reservedHours / totalAvailableHours) * 100m, 2, MidpointRounding.AwayFromZero);

        return new SpaceReportResponse
        {
            SpaceId = space.Id,
            SpaceName = space.Name,
            OccupancyRate = occupancyRate,
            TotalIncome = decimal.Round(spaceReservations.Sum(GetNetIncome), 2, MidpointRounding.AwayFromZero),
            TotalReservations = spaceReservations.Count
        };

        double GetOverlappedHours(Reservation reservation)
        {
            var effectiveStart = reservation.StartAt < from ? from : reservation.StartAt;
            var effectiveEnd = reservation.EndAt > toExclusive ? toExclusive : reservation.EndAt;
            return Math.Max(0, (effectiveEnd - effectiveStart).TotalHours);
        }
    }

    private static double CalculateAvailableHours(Space space, DateTime from, DateTime toExclusive)
    {
        var total = 0d;
        for (var day = from.Date; day < toExclusive.Date; day = day.AddDays(1))
        {
            total += (space.ClosingTime.ToTimeSpan() - space.OpeningTime.ToTimeSpan()).TotalHours;
        }

        if (from.TimeOfDay != TimeSpan.Zero || toExclusive.TimeOfDay != TimeSpan.Zero)
        {
            var totalDays = (toExclusive - from).TotalDays;
            if (totalDays > 0)
            {
                total = (space.ClosingTime.ToTimeSpan() - space.OpeningTime.ToTimeSpan()).TotalHours * totalDays;
            }
        }

        return total;
    }

    private static decimal GetNetIncome(Reservation reservation)
    {
        return reservation.FinalPrice - (reservation.RefundAmount ?? 0m);
    }

    private static IEnumerable<DateOnly> EnumerateDays(DateTime from, DateTime toExclusive)
    {
        var startDate = DateOnly.FromDateTime(from.Date);
        var lastIncludedDate = DateOnly.FromDateTime(toExclusive.AddTicks(-1).Date);

        for (var date = startDate; date <= lastIncludedDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    private static string GetMostDemandedHour(IReadOnlyCollection<Reservation> reservations)
    {
        if (reservations.Count == 0)
        {
            return string.Empty;
        }

        var hourGroup = reservations
            .GroupBy(reservation => reservation.StartAt.Hour)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .First();

        return $"{hourGroup.Key:00}:00";
    }
}
