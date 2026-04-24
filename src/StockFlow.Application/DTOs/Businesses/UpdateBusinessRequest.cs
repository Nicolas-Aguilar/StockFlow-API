using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Businesses;

public sealed class UpdateBusinessRequest
{
    [Required, MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
