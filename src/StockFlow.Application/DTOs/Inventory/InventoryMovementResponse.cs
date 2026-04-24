using StockFlow.Domain.Enums;

namespace StockFlow.Application.DTOs.Inventory;

public sealed record InventoryMovementResponse(Guid Id, Guid BusinessId, Guid ProductId, string ProductName, InventoryMovementType MovementType, int Quantity, string? Reason, DateTime CreatedAt);
