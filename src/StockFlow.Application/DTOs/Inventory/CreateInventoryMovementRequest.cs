using System.ComponentModel.DataAnnotations;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.DTOs.Inventory;

public sealed class CreateInventoryMovementRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Required]
    public InventoryMovementType MovementType { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [MaxLength(500)]
    public string? Reason { get; init; }
}
