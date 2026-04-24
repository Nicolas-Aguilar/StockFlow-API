using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.DTOs.Sales;

public sealed class CreateSaleItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
