using FluentValidation;

namespace CoworkSpaces.Application.Features.Spaces.Commands.UpdateSpace;

public class UpdateSpaceCommandValidator : AbstractValidator<UpdateSpaceCommand>
{
    public UpdateSpaceCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("El id del espacio es obligatorio.");

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
