using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Reports;

namespace StockFlow.Application.Interfaces;

public interface IReportService
{
    Task<PagedResponse<ProductResponse>> GetLowStockProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetExpiringSoonProductsAsync(PaginationQuery paginationQuery, int days = 30, CancellationToken cancellationToken = default);
    Task<PagedResponse<ProductResponse>> GetExpiredProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<PagedResponse<TopSellingProductResponse>> GetTopSellingProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default);
    Task<SalesSummaryResponse> GetSalesSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default);
    Task<ProfitSummaryResponse> GetProfitSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default);
    Task<InventoryValuationResponse> GetInventoryValuationAsync(CancellationToken cancellationToken = default);
}
