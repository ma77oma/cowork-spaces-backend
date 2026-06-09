using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Domain.Services;

public class CancellationPolicyService
{
    public decimal CalculateRefundAmount(decimal finalPrice, DateTime startAt, DateTime cancelledAt, ReservationStatus status)
    {
        if (status == ReservationStatus.Completed)
        {
            throw new InvalidOperationException("Una reserva completada no se puede cancelar.");
        }

        var hoursBeforeStart = (startAt - cancelledAt).TotalHours;

        if (hoursBeforeStart > 48)
        {
            return finalPrice;
        }

        if (hoursBeforeStart >= 24)
        {
            return decimal.Round(finalPrice * 0.50m, 2, MidpointRounding.AwayFromZero);
        }

        return 0m;
    }
}
