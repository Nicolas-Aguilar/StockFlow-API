using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(150)]
    public string BusinessName { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? BusinessDescription { get; init; }
}
