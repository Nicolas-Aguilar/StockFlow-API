using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Inventory;

namespace StockFlow.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryMovementResponse> CreateMovementAsync(CreateInventoryMovementRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryMovementResponse>> GetMovementsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<PagedResponse<InventoryMovementResponse>> GetProductHistoryAsync(Guid productId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
}
