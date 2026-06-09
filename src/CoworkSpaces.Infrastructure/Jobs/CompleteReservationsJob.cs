using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CoworkSpaces.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class CompleteReservationsJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CompleteReservationsJob> _logger;

    public CompleteReservationsJob(AppDbContext dbContext, ILogger<CompleteReservationsJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.Now;

        var reservationsToComplete = await _dbContext.Reservations
            .Where(reservation => reservation.Status == ReservationStatus.Confirmed && reservation.EndAt <= now)
            .ToListAsync(context.CancellationToken);

        if (reservationsToComplete.Count == 0)
        {
            _logger.LogInformation("CompleteReservationsJob ejecutado sin reservas pendientes por completar.");
            return;
        }

        foreach (var reservation in reservationsToComplete)
        {
            reservation.Status = ReservationStatus.Completed;
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "CompleteReservationsJob completó {ReservationCount} reservas vencidas a las {ExecutionTime}.",
            reservationsToComplete.Count,
            now);
    }
}
