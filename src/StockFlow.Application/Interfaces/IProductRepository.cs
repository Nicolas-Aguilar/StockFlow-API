using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Reports;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetAllAsync(Guid businessId, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPagedAsync(Guid businessId, PaginationQuery paginationQuery, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(Guid businessId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> SearchAsync(Guid businessId, string term, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> SearchPagedAsync(Guid businessId, string term, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetLowStockAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetLowStockPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetExpiringSoonAsync(Guid businessId, DateTime currentDateUtc, DateTime limitDateUtc, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetExpiringSoonPagedAsync(Guid businessId, DateTime currentDateUtc, DateTime limitDateUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetExpiredAsync(Guid businessId, DateTime currentDateUtc, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetExpiredPagedAsync(Guid businessId, DateTime currentDateUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<InventoryValuationTotalsResponse> GetInventoryValuationTotalsAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<bool> ExistsInternalCodeAsync(Guid businessId, string internalCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasHistoryAsync(Guid productId, Guid businessId, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateStockAsync(Guid productId, Guid businessId, int quantityDelta, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
