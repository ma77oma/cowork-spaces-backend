using CoworkSpaces.Application.Features.Spaces.Commands.CreateSpace;

namespace CoworkSpaces.Tests.Validators;

public class CreateSpaceCommandValidatorTests
{
    private readonly CreateSpaceCommandValidator _validator = new();

    [Fact]
    public async Task ShouldFailWhenNameIsEmpty()
    {
        var command = new CreateSpaceCommand { Name = string.Empty, Capacity = 1, BaseHourlyRate = 10, OpeningTime = new TimeOnly(8, 0), ClosingTime = new TimeOnly(18, 0) };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ShouldFailWhenOpeningTimeIsGreaterThanClosingTime()
    {
        var command = new CreateSpaceCommand { Name = "Sala", Capacity = 1, BaseHourlyRate = 10, OpeningTime = new TimeOnly(18, 0), ClosingTime = new TimeOnly(8, 0) };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }
}
