using StockFlow.Domain.Entities;

namespace StockFlow.Application.Interfaces;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Category>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid businessId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, Guid businessId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
