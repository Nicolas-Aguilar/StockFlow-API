using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(Product product, CancellationToken cancellationToken = default) => _dbContext.Products.AddAsync(product, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(Guid businessId, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.Include(product => product.Category).Where(product => product.BusinessId == businessId);
        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        return await query.OrderBy(product => product.Name).ToArrayAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) => _dbContext.Products.Include(product => product.Category).FirstOrDefaultAsync(product => product.Id == id && product.BusinessId == businessId, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> SearchAsync(Guid businessId, string term, CancellationToken cancellationToken = default)
    {
        var normalized = term.Trim().ToLower();
        return await _dbContext.Products.Include(product => product.Category)
            .Where(product => product.BusinessId == businessId && (product.Name.ToLower().Contains(normalized) || product.InternalCode.ToLower().Contains(normalized)))
            .OrderBy(product => product.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetLowStockAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await GetQuery(businessId).Where(product => product.CurrentStock <= product.MinimumStock).OrderBy(product => product.CurrentStock).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetExpiringSoonAsync(Guid businessId, DateTime limitDateUtc, CancellationToken cancellationToken = default)
        => await GetQuery(businessId).Where(product => product.ExpirationDate.HasValue && product.ExpirationDate.Value.Date >= DateTime.UtcNow.Date && product.ExpirationDate.Value.Date <= limitDateUtc.Date).OrderBy(product => product.ExpirationDate).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetExpiredAsync(Guid businessId, DateTime currentDateUtc, CancellationToken cancellationToken = default)
        => await GetQuery(businessId).Where(product => product.ExpirationDate.HasValue && product.ExpirationDate.Value.Date < currentDateUtc.Date).OrderBy(product => product.ExpirationDate).ToArrayAsync(cancellationToken);

    public Task<bool> ExistsInternalCodeAsync(Guid businessId, string internalCode, Guid? excludeId = null, CancellationToken cancellationToken = default) => _dbContext.Products.AnyAsync(product => product.BusinessId == businessId && product.InternalCode == internalCode && (!excludeId.HasValue || product.Id != excludeId), cancellationToken);

    public async Task<bool> HasHistoryAsync(Guid productId, Guid businessId, CancellationToken cancellationToken = default)
    {
        var hasMovements = await _dbContext.InventoryMovements.AnyAsync(movement => movement.ProductId == productId && movement.BusinessId == businessId, cancellationToken);
        if (hasMovements)
        {
            return true;
        }

        return await _dbContext.SaleItems.AnyAsync(item => item.ProductId == productId && item.Sale != null && item.Sale.BusinessId == businessId, cancellationToken);
    }

    public void Remove(Product product) => _dbContext.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Product> GetQuery(Guid businessId) => _dbContext.Products.Include(product => product.Category).Where(product => product.BusinessId == businessId);
}
