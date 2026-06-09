using FluentValidation;

namespace CoworkSpaces.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo no es válido.");

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
