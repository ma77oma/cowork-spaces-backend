using CoworkSpaces.Application.Features.Reservations.Commands.CreateReservation;

namespace CoworkSpaces.Tests.Validators;

public class CreateReservationCommandValidatorTests
{
    private readonly CreateReservationCommandValidator _validator = new();

    [Fact]
    public async Task ShouldFailWhenSpaceIdIsEmpty()
    {
        var command = new CreateReservationCommand { SpaceId = Guid.Empty, StartAt = DateTime.UtcNow.AddHours(1), EndAt = DateTime.UtcNow.AddHours(2) };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ShouldFailWhenStartAtIsNotBeforeEndAt()
    {
        var now = DateTime.UtcNow.AddHours(1);
        var command = new CreateReservationCommand { SpaceId = Guid.NewGuid(), StartAt = now, EndAt = now };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }
}
