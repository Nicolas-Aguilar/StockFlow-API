using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Sales;

namespace StockFlow.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<SaleResponse>> GetAllAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<SaleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<SaleResponse>> GetByDateRangeAsync(DateRangeQuery query, PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
}
