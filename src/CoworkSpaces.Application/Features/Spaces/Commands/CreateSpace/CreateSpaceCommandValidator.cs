using FluentValidation;

namespace CoworkSpaces.Application.Features.Spaces.Commands.CreateSpace;

public class CreateSpaceCommandValidator : AbstractValidator<CreateSpaceCommand>
{
    public CreateSpaceCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(command => command.Capacity)
            .GreaterThan(0).WithMessage("La capacidad debe ser mayor que 0.");

        RuleFor(command => command.BaseHourlyRate)
            .GreaterThan(0).WithMessage("La tarifa base por hora debe ser mayor que 0.");

        RuleFor(command => command.OpeningTime)
            .LessThan(command => command.ClosingTime)
            .WithMessage("La hora de apertura debe ser menor que la hora de cierre.");
    }
}
