using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public sealed class InventoryMovement : Entity
{
    public Guid BusinessId { get; set; }
    public Guid ProductId { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business? Business { get; set; }
    public Product? Product { get; set; }
}
