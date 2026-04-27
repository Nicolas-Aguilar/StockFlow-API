using StockFlow.Application.Common;
using StockFlow.Domain.Entities;
using StockFlow.Application.DTOs.Reports;

namespace StockFlow.Application.Interfaces;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Sale>> GetAllAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<PagedResult<Sale>> GetPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, Guid businessId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Sale>> GetByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<PagedResult<Sale>> GetByDateRangePagedAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<SalesTotalsResponse> GetSalesTotalsByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<ProfitTotalsResponse> GetProfitTotalsByDateRangeAsync(Guid businessId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TopSellingProductResponse>> GetTopSellingProductsAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<PagedResult<TopSellingProductResponse>> GetTopSellingProductsPagedAsync(Guid businessId, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
