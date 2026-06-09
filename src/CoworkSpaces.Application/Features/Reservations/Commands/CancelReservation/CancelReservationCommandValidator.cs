using FluentValidation;

namespace CoworkSpaces.Application.Features.Reservations.Commands.CancelReservation;

public class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(command => command.ReservationId)
            .NotEmpty().WithMessage("La reserva es obligatoria.");
    }
}
