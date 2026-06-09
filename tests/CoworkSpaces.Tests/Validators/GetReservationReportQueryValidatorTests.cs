using CoworkSpaces.Application.Features.Reports.Queries.GetReservationReport;

namespace CoworkSpaces.Tests.Validators;

public class GetReservationReportQueryValidatorTests
{
    private readonly GetReservationReportQueryValidator _validator = new();

    [Fact]
    public async Task ShouldFailWhenFromIsGreaterThanTo()
    {
        var query = new GetReservationReportQuery { From = new DateTime(2026, 6, 30), To = new DateTime(2026, 6, 1) };
        var result = await _validator.ValidateAsync(query);
        Assert.False(result.IsValid);
    }
}
