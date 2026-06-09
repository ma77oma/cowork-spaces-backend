using FluentValidation;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(command => command.SpaceId)
            .NotEmpty().WithMessage("El espacio es obligatorio.");

        RuleFor(command => command.StartAt)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(command => command.EndAt)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria.");

        RuleFor(command => command)
            .Must(command => command.StartAt < command.EndAt)
            .WithMessage("La fecha de inicio debe ser menor que la fecha de fin.");
    }
}
