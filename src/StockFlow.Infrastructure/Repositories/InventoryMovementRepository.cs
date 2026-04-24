using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Repositories;

public sealed class InventoryMovementRepository : IInventoryMovementRepository
{
    private readonly AppDbContext _dbContext;

    public InventoryMovementRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(InventoryMovement movement, CancellationToken cancellationToken = default) => _dbContext.InventoryMovements.AddAsync(movement, cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<InventoryMovement> movements, CancellationToken cancellationToken = default) => _dbContext.InventoryMovements.AddRangeAsync(movements, cancellationToken);

    public async Task<IReadOnlyCollection<InventoryMovement>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default) => await _dbContext.InventoryMovements.Include(movement => movement.Product).Where(movement => movement.BusinessId == businessId).OrderByDescending(movement => movement.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<InventoryMovement>> GetProductHistoryAsync(Guid businessId, Guid productId, CancellationToken cancellationToken = default) => await _dbContext.InventoryMovements.Include(movement => movement.Product).Where(movement => movement.BusinessId == businessId && movement.ProductId == productId).OrderByDescending(movement => movement.CreatedAt).ToArrayAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);
}
