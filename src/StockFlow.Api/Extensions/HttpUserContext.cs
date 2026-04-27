using System.Security.Claims;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Api.Extensions;

public sealed class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => GetGuidClaim("userId");
    public Guid BusinessId => GetGuidClaim("businessId");
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private Guid GetGuidClaim(string claimType)
    {
        if (!IsAuthenticated)
        {
            throw new InvalidUserContextException("Authentication is required to access this resource.");
        }

        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidUserContextException($"Authenticated user is missing the required '{claimType}' claim.");
        }

        if (!Guid.TryParse(value, out var parsed))
        {
            throw new InvalidUserContextException($"Authenticated user has an invalid '{claimType}' claim.");
        }

        return parsed;
    }
}
