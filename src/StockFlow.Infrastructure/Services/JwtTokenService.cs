using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Application.Interfaces;

namespace StockFlow.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider dateTimeProvider)
    {
        _options = options.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(Guid userId, Guid businessId, string email)
    {
        var expires = _dateTimeProvider.UtcNow.AddMinutes(_options.ExpirationMinutes);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("userId", userId.ToString()),
            new("businessId", businessId.ToString())
        };

        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
