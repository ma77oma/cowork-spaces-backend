using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Domain.Services;

namespace CoworkSpaces.Tests;

public class PricingServiceTests
{
    private readonly PricingService _service = new();

    [Fact]
    public void ShouldCalculateBasePriceWithoutAdjustments()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 3, 12, 0, 0), new DateTime(2026, 6, 3, 14, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(200m, result);
    }

    [Fact]
    public void ShouldApplyMorningPeakIncrease()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 3, 9, 30, 0), new DateTime(2026, 6, 3, 10, 30, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(125m, result);
    }

    [Fact]
    public void ShouldApplyEveningPeakIncrease()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 3, 17, 30, 0), new DateTime(2026, 6, 3, 18, 30, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(125m, result);
    }

    [Fact]
    public void ShouldApplyWeekendIncrease()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 6, 12, 0, 0), new DateTime(2026, 6, 6, 14, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(230m, result);
    }

    [Fact]
    public void ShouldApplyLongReservationDiscount()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 3, 12, 0, 0), new DateTime(2026, 6, 3, 16, 0, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(360m, result);
    }

    [Fact]
    public void ShouldApplyAdvanceDiscountAtSevenDaysOrMore()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 10, 12, 0, 0), new DateTime(2026, 6, 10, 14, 0, 0), new DateTime(2026, 6, 3, 12, 0, 0));
        Assert.Equal(190m, result);
    }

    [Fact]
    public void ShouldApplyCombinedRulesInCorrectOrder()
    {
        var result = _service.CalculateFinalPrice(100m, new DateTime(2026, 6, 13, 9, 0, 0), new DateTime(2026, 6, 13, 13, 0, 0), new DateTime(2026, 6, 1, 8, 0, 0));
        Assert.Equal(491.63m, result);
    }

    [Fact]
    public void ShouldRoundToTwoDecimals()
    {
        var result = _service.CalculateFinalPrice(99.99m, new DateTime(2026, 6, 3, 9, 0, 0), new DateTime(2026, 6, 3, 10, 10, 0), new DateTime(2026, 6, 1, 10, 0, 0));
        Assert.Equal(145.82m, result);
    }
}
