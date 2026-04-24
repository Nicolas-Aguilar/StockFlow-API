using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IInventoryMovementRepository
{
    Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<InventoryMovement> movements, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovement>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventoryMovement>> GetProductHistoryAsync(Guid businessId, Guid productId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
