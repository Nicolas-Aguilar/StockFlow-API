using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
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

    public async Task<IReadOnlyCollection<InventoryMovement>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default) => await GetByBusinessQuery(businessId).ToArrayAsync(cancellationToken);

    public Task<PagedResult<InventoryMovement>> GetByBusinessPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetByBusinessQuery(businessId), paginationQuery, cancellationToken);

    public async Task<IReadOnlyCollection<InventoryMovement>> GetProductHistoryAsync(Guid businessId, Guid productId, CancellationToken cancellationToken = default) => await GetProductHistoryQuery(businessId, productId).ToArrayAsync(cancellationToken);

    public Task<PagedResult<InventoryMovement>> GetProductHistoryPagedAsync(Guid businessId, Guid productId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetProductHistoryQuery(businessId, productId), paginationQuery, cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<InventoryMovement> GetByBusinessQuery(Guid businessId)
        => _dbContext.InventoryMovements.Include(movement => movement.Product).Where(movement => movement.BusinessId == businessId).OrderByDescending(movement => movement.CreatedAt).ThenByDescending(movement => movement.Id);

    private IQueryable<InventoryMovement> GetProductHistoryQuery(Guid businessId, Guid productId)
        => _dbContext.InventoryMovements.Include(movement => movement.Product).Where(movement => movement.BusinessId == businessId && movement.ProductId == productId).OrderByDescending(movement => movement.CreatedAt).ThenByDescending(movement => movement.Id);

    private static async Task<PagedResult<InventoryMovement>> ToPagedResultAsync(IQueryable<InventoryMovement> query, PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paginationQuery.Skip).Take(paginationQuery.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<InventoryMovement>(items, paginationQuery.Page, paginationQuery.PageSize, totalItems);
    }
}
