using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Products;

public sealed class UpdateProductRequest
{
    [Required]
    public Guid CategoryId { get; init; }

    [Required, MaxLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string InternalCode { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(0, double.MaxValue)]
    public decimal PurchasePrice { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal SalePrice { get; init; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; init; }

    public DateTime? ExpirationDate { get; init; }

    public bool IsActive { get; init; } = true;
}
