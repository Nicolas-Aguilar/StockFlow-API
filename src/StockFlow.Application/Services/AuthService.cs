using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Auth;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;
using StockFlow.Application.Interfaces;

namespace StockFlow.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IUserContext _userContext;

    public AuthService(
        IUserRepository userRepository,
        IBusinessRepository businessRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IUserContext userContext)
    {
        _userRepository = userRepository;
        _businessRepository = businessRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _userContext = userContext;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new ValidationDomainException("The email is already registered.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            IsActive = true
        };

        user.PasswordHash = _passwordService.HashPassword(user, request.Password);

        var business = new Business
        {
            OwnerUserId = user.Id,
            Name = request.BusinessName.Trim(),
            Description = request.BusinessDescription?.Trim(),
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _businessRepository.AddAsync(business, cancellationToken);
        await _businessRepository.SaveChangesAsync(cancellationToken);

        var token = _tokenService.GenerateToken(user.Id, business.Id, user.Email);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, user.ToProfileResponse(), business.ToResponse());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new ValidationDomainException("Invalid credentials.");

        if (!user.IsActive || !_passwordService.VerifyPassword(user, request.Password, user.PasswordHash))
        {
            throw new ValidationDomainException("Invalid credentials.");
        }

        var business = await _businessRepository.GetByOwnerUserIdAsync(user.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Business was not found for the authenticated user.");

        var token = _tokenService.GenerateToken(user.Id, business.Id, user.Email);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, user.ToProfileResponse(), business.ToResponse());
    }

    public async Task<UserProfileResponse> MeAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var user = await _userRepository.GetByIdAsync(_userContext.UserId, cancellationToken)
            ?? throw new EntityNotFoundException("Authenticated user was not found.");

        return user.ToProfileResponse();
    }

    private void EnsureAuthenticated()
    {
        if (!_userContext.IsAuthenticated)
        {
            throw new BusinessAccessDeniedException("Authentication is required.");
        }
    }
}
