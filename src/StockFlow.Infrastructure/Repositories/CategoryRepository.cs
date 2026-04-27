using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(Category category, CancellationToken cancellationToken = default) => _dbContext.Categories.AddAsync(category, cancellationToken).AsTask();

    public async Task<PagedResult<Category>> GetPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categories
            .Where(category => category.BusinessId == businessId)
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paginationQuery.Skip).Take(paginationQuery.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<Category>(items, paginationQuery.Page, paginationQuery.PageSize, totalItems);
    }

    public Task<Category?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) => _dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id && category.BusinessId == businessId, cancellationToken);

    public Task<bool> ExistsByNameAsync(Guid businessId, string name, Guid? excludeId = null, CancellationToken cancellationToken = default) => _dbContext.Categories.AnyAsync(category => category.BusinessId == businessId && category.Name == name && (!excludeId.HasValue || category.Id != excludeId), cancellationToken);

    public Task<bool> HasProductsAsync(Guid categoryId, Guid businessId, CancellationToken cancellationToken = default) => _dbContext.Products.AnyAsync(product => product.CategoryId == categoryId && product.BusinessId == businessId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dbContext.SaveChangesAsync(cancellationToken);
}
