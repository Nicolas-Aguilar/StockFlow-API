using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Categories;

public sealed class CreateCategoryRequest
{
    [Required, MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
