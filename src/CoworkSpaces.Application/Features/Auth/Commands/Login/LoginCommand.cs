using CoworkSpaces.Application.DTOs.Auth;
using MediatR;

namespace CoworkSpaces.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
