using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}
