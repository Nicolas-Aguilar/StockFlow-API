using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetAllAsync(Guid businessId, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> SearchAsync(Guid businessId, string term, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetLowStockAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetExpiringSoonAsync(Guid businessId, DateTime limitDateUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetExpiredAsync(Guid businessId, DateTime currentDateUtc, CancellationToken cancellationToken = default);
    Task<bool> ExistsInternalCodeAsync(Guid businessId, string internalCode, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasHistoryAsync(Guid productId, Guid businessId, CancellationToken cancellationToken = default);
    void Remove(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
