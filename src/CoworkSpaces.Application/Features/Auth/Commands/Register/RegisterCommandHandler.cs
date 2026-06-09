using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Auth;
using MediatR;

namespace CoworkSpaces.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return _identityService.RegisterAsync(request.FullName.Trim(), request.Email.Trim(), request.Password, cancellationToken);
    }
}
