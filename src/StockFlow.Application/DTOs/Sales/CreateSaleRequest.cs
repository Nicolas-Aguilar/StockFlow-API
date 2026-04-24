using System.ComponentModel.DataAnnotations;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.DTOs.Sales;

public sealed class CreateSaleRequest
{
    [Required]
    public PaymentMethod PaymentMethod { get; init; }

    [MinLength(1)]
    public IReadOnlyCollection<CreateSaleItemRequest> Items { get; init; } = Array.Empty<CreateSaleItemRequest>();
}
