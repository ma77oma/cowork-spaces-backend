using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Domain.Services;

namespace CoworkSpaces.Tests;

public class CancellationPolicyServiceTests
{
    private readonly CancellationPolicyService _service = new();
    private const decimal FinalPrice = 200m;
    private static readonly DateTime StartAt = new(2026, 6, 10, 12, 0, 0);

    [Fact]
    public void ShouldReturnFullRefundWhenMoreThan48Hours()
    {
        var result = _service.CalculateRefundAmount(FinalPrice, StartAt, new DateTime(2026, 6, 8, 11, 59, 59), ReservationStatus.Confirmed);
        Assert.Equal(200m, result);
    }

    [Fact]
    public void ShouldReturnHalfRefundAtExactly48Hours()
    {
        var result = _service.CalculateRefundAmount(FinalPrice, StartAt, new DateTime(2026, 6, 8, 12, 0, 0), ReservationStatus.Confirmed);
        Assert.Equal(100m, result);
    }

    [Fact]
    public void ShouldReturnHalfRefundBetween24And48Hours()
    {
        var result = _service.CalculateRefundAmount(FinalPrice, StartAt, new DateTime(2026, 6, 9, 0, 0, 0), ReservationStatus.Confirmed);
        Assert.Equal(100m, result);
    }

    [Fact]
    public void ShouldReturnHalfRefundAtExactly24Hours()
    {
        var result = _service.CalculateRefundAmount(FinalPrice, StartAt, new DateTime(2026, 6, 9, 12, 0, 0), ReservationStatus.Confirmed);
        Assert.Equal(100m, result);
    }

    [Fact]
    public void ShouldReturnZeroRefundWhenLessThan24Hours()
    {
        var result = _service.CalculateRefundAmount(FinalPrice, StartAt, new DateTime(2026, 6, 9, 12, 0, 1), ReservationStatus.Confirmed);
        Assert.Equal(0m, result);
    }
}
