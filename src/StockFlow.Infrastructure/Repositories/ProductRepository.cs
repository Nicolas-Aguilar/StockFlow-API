using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
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
        var query = GetAllQuery(businessId, categoryId);
        return await query.ToArrayAsync(cancellationToken);
    }

    public Task<PagedResult<Product>> GetPagedAsync(Guid businessId, PaginationQuery paginationQuery, Guid? categoryId = null, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetAllQuery(businessId, categoryId), paginationQuery, cancellationToken);

    private IQueryable<Product> GetAllQuery(Guid businessId, Guid? categoryId)
    {
        var query = _dbContext.Products.Include(product => product.Category).Where(product => product.BusinessId == businessId);
        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        return query.OrderBy(product => product.Name).ThenBy(product => product.Id);
    }

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(Guid businessId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToArray();
        return await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.BusinessId == businessId && ids.Contains(product.Id))
            .ToArrayAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) => _dbContext.Products.Include(product => product.Category).FirstOrDefaultAsync(product => product.Id == id && product.BusinessId == businessId, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> SearchAsync(Guid businessId, string term, CancellationToken cancellationToken = default)
    {
        var normalized = term.Trim().ToLower();
        return await GetSearchQuery(businessId, normalized).ToArrayAsync(cancellationToken);
    }

    public Task<PagedResult<Product>> SearchPagedAsync(Guid businessId, string term, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetSearchQuery(businessId, term.Trim().ToLower()), paginationQuery, cancellationToken);

    private IQueryable<Product> GetSearchQuery(Guid businessId, string normalized)
        => _dbContext.Products.Include(product => product.Category)
            .Where(product => product.BusinessId == businessId && (product.Name.ToLower().Contains(normalized) || product.InternalCode.ToLower().Contains(normalized)))
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id);

    public async Task<IReadOnlyCollection<Product>> GetLowStockAsync(Guid businessId, CancellationToken cancellationToken = default)
        => await GetLowStockQuery(businessId).ToArrayAsync(cancellationToken);

    public Task<PagedResult<Product>> GetLowStockPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetLowStockQuery(businessId), paginationQuery, cancellationToken);

    private IQueryable<Product> GetLowStockQuery(Guid businessId)
        => GetQuery(businessId).Where(product => product.CurrentStock <= product.MinimumStock).OrderBy(product => product.CurrentStock).ThenBy(product => product.Name).ThenBy(product => product.Id);

    public async Task<IReadOnlyCollection<Product>> GetExpiringSoonAsync(Guid businessId, DateTime currentDateUtc, DateTime limitDateUtc, CancellationToken cancellationToken = default)
        => await GetExpiringSoonQuery(businessId, currentDateUtc, limitDateUtc).ToArrayAsync(cancellationToken);

    public Task<PagedResult<Product>> GetExpiringSoonPagedAsync(Guid businessId, DateTime currentDateUtc, DateTime limitDateUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetExpiringSoonQuery(businessId, currentDateUtc, limitDateUtc), paginationQuery, cancellationToken);

    private IQueryable<Product> GetExpiringSoonQuery(Guid businessId, DateTime currentDateUtc, DateTime limitDateUtc)
        => GetQuery(businessId).Where(product => product.ExpirationDate.HasValue && product.ExpirationDate.Value.Date >= currentDateUtc.Date && product.ExpirationDate.Value.Date <= limitDateUtc.Date).OrderBy(product => product.ExpirationDate).ThenBy(product => product.Name).ThenBy(product => product.Id);

    public async Task<IReadOnlyCollection<Product>> GetExpiredAsync(Guid businessId, DateTime currentDateUtc, CancellationToken cancellationToken = default)
        => await GetExpiredQuery(businessId, currentDateUtc).ToArrayAsync(cancellationToken);

    public Task<PagedResult<Product>> GetExpiredPagedAsync(Guid businessId, DateTime currentDateUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetExpiredQuery(businessId, currentDateUtc), paginationQuery, cancellationToken);

    private IQueryable<Product> GetExpiredQuery(Guid businessId, DateTime currentDateUtc)
        => GetQuery(businessId).Where(product => product.ExpirationDate.HasValue && product.ExpirationDate.Value.Date < currentDateUtc.Date).OrderBy(product => product.ExpirationDate).ThenBy(product => product.Name).ThenBy(product => product.Id);

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

    public async Task<bool> TryUpdateStockAsync(Guid productId, Guid businessId, int quantityDelta, DateTime updatedAtUtc, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.Where(product => product.Id == productId && product.BusinessId == businessId);

        if (quantityDelta < 0)
        {
            query = query.Where(product => product.CurrentStock >= -quantityDelta);
        }

        var affectedRows = await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(product => product.CurrentStock, product => product.CurrentStock + quantityDelta)
            .SetProperty(product => product.UpdatedAt, _ => updatedAtUtc), cancellationToken);

        return affectedRows == 1;
    }

    public void Remove(Product product) => _dbContext.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Product> GetQuery(Guid businessId) => _dbContext.Products.Include(product => product.Category).Where(product => product.BusinessId == businessId);

    private static async Task<PagedResult<Product>> ToPagedResultAsync(IQueryable<Product> query, PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paginationQuery.Skip).Take(paginationQuery.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<Product>(items, paginationQuery.Page, paginationQuery.PageSize, totalItems);
    }
}
