using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
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

    public Task<PagedResult<Sale>> GetPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetSalesByBusinessQuery(businessId), paginationQuery, cancellationToken);

    public Task<Sale?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default) => GetSalesQuery().FirstOrDefaultAsync(sale => sale.Id == id && sale.BusinessId == businessId, cancellationToken);

    public async Task<IReadOnlyCollection<Sale>> GetByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => await GetSalesQuery().Where(sale => sale.BusinessId == businessId && sale.CreatedAt >= fromUtc && sale.CreatedAt <= toUtc).OrderByDescending(sale => sale.CreatedAt).ToArrayAsync(cancellationToken);

    public Task<PagedResult<Sale>> GetByDateRangePagedAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
        => ToPagedResultAsync(GetSalesByDateRangeQuery(businessId, fromUtc, toUtc), paginationQuery, cancellationToken);

    public async Task<SalesTotalsResponse> GetSalesTotalsByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var aggregate = await _dbContext.Sales
            .Where(sale => sale.BusinessId == businessId && sale.CreatedAt >= fromUtc && sale.CreatedAt <= toUtc)
            .GroupBy(_ => 1)
            .Select(group => new SalesTotalsResponse(group.Count(), group.Sum(sale => sale.Total)))
            .SingleOrDefaultAsync(cancellationToken);

        return aggregate ?? new SalesTotalsResponse(0, 0m);
    }

    public async Task<ProfitTotalsResponse> GetProfitTotalsByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var aggregate = await _dbContext.Sales
            .Where(sale => sale.BusinessId == businessId && sale.CreatedAt >= fromUtc && sale.CreatedAt <= toUtc)
            .GroupBy(_ => 1)
            .Select(group => new ProfitTotalsResponse(group.Count(), group.Sum(sale => sale.EstimatedProfit)))
            .SingleOrDefaultAsync(cancellationToken);

        return aggregate ?? new ProfitTotalsResponse(0, 0m);
    }

    public async Task<IReadOnlyCollection<TopSellingProductResponse>> GetTopSellingProductsAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await GetTopSellingProductsQuery(businessId).Take(10).ToArrayAsync(cancellationToken);
    }

    public async Task<PagedResult<TopSellingProductResponse>> GetTopSellingProductsPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var query = GetTopSellingProductsQuery(businessId);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paginationQuery.Skip).Take(paginationQuery.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<TopSellingProductResponse>(items, paginationQuery.Page, paginationQuery.PageSize, totalItems);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private IQueryable<Sale> GetSalesQuery() => _dbContext.Sales.Include(sale => sale.Items).ThenInclude(item => item.Product);

    private IQueryable<Sale> GetSalesByBusinessQuery(Guid businessId)
        => GetSalesQuery().Where(sale => sale.BusinessId == businessId).OrderByDescending(sale => sale.CreatedAt).ThenByDescending(sale => sale.Id);

    private IQueryable<Sale> GetSalesByDateRangeQuery(Guid businessId, DateTime fromUtc, DateTime toUtc)
        => GetSalesQuery().Where(sale => sale.BusinessId == businessId && sale.CreatedAt >= fromUtc && sale.CreatedAt <= toUtc).OrderByDescending(sale => sale.CreatedAt).ThenByDescending(sale => sale.Id);

    private IQueryable<TopSellingProductResponse> GetTopSellingProductsQuery(Guid businessId)
        => _dbContext.SaleItems
            .Where(item => item.Sale!.BusinessId == businessId)
            .GroupBy(item => new { item.ProductId, ProductName = item.Product!.Name, item.Product.InternalCode })
            .Select(group => new TopSellingProductResponse(group.Key.ProductId, group.Key.ProductName, group.Key.InternalCode, group.Sum(item => item.Quantity), group.Sum(item => item.Subtotal)))
            .OrderByDescending(item => item.TotalUnitsSold)
            .ThenBy(item => item.ProductName)
            .ThenBy(item => item.ProductId);

    private static async Task<PagedResult<Sale>> ToPagedResultAsync(IQueryable<Sale> query, PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip(paginationQuery.Skip).Take(paginationQuery.PageSize).ToArrayAsync(cancellationToken);
        return new PagedResult<Sale>(items, paginationQuery.Page, paginationQuery.PageSize, totalItems);
    }
}
