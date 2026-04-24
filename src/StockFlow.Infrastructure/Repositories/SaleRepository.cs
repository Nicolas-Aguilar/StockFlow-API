using Microsoft.EntityFrameworkCore;
using StockFlow.Application.DTOs.Reports;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Data;

namespace StockFlow.Infrastructure.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _dbContext;

    public SaleRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) => _dbContext.Sales.AddAsync(sale, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<Sale>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default) => await GetSalesQuery().Where(sale => sale.BusinessId == businessId).OrderByDescending(sale => sale.CreatedAt).ToArrayAsync(cancellationToken);

    public Task<Sale?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) => GetSalesQuery().FirstOrDefaultAsync(sale => sale.Id == id && sale.BusinessId == businessId, cancellationToken);

    public async Task<IReadOnlyCollection<Sale>> GetByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => await GetSalesQuery().Where(sale => sale.BusinessId == businessId && sale.CreatedAt >= fromUtc && sale.CreatedAt <= toUtc).OrderByDescending(sale => sale.CreatedAt).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<TopSellingProductResponse>> GetTopSellingProductsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaleItems
            .Where(item => item.Sale!.BusinessId == businessId)
            .GroupBy(item => new { item.ProductId, ProductName = item.Product!.Name, item.Product.InternalCode })
            .Select(group => new TopSellingProductResponse(group.Key.ProductId, group.Key.ProductName, group.Key.InternalCode, group.Sum(item => item.Quantity), group.Sum(item => item.Subtotal)))
            .OrderByDescending(item => item.TotalUnitsSold)
            .Take(10)
            .ToArrayAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<Sale> GetSalesQuery() => _dbContext.Sales.Include(sale => sale.Items).ThenInclude(item => item.Product);
}
