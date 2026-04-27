using StockFlow.Application.Common;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IInventoryMovementRepository
{
    Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<InventoryMovement> movements, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovement>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<PagedResult<InventoryMovement>> GetByBusinessPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovement>> GetProductHistoryAsync(Guid businessId, Guid productId, CancellationToken cancellationToken = default);
    Task<PagedResult<InventoryMovement>> GetProductHistoryPagedAsync(Guid businessId, Guid productId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
