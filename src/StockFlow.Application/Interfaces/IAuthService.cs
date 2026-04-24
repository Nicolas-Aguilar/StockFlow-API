using StockFlow.Application.DTOs.Auth;

namespace StockFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> MeAsync(CancellationToken cancellationToken = default);
}
