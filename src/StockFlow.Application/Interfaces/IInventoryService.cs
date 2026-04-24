using StockFlow.Application.DTOs.Inventory;

namespace StockFlow.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryMovementResponse> CreateMovementAsync(CreateInventoryMovementRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementResponse>> GetMovementsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovementResponse>> GetProductHistoryAsync(Guid productId, CancellationToken cancellationToken = default);
}
