using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Auth;
using MediatR;

namespace CoworkSpaces.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return _identityService.LoginAsync(request.Email.Trim(), request.Password, cancellationToken);
    }
}
