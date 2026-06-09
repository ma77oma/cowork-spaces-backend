using CoworkSpaces.Application.DTOs.Auth;

namespace CoworkSpaces.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResponse> RegisterAsync(string fullName, string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
