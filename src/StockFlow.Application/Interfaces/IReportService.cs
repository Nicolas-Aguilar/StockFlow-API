using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Reports;

namespace StockFlow.Application.Interfaces;

public interface IReportService
{
    Task<IReadOnlyCollection<ProductResponse>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetExpiringSoonProductsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductResponse>> GetExpiredProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TopSellingProductResponse>> GetTopSellingProductsAsync(CancellationToken cancellationToken = default);
    Task<SalesSummaryResponse> GetSalesSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default);
    Task<ProfitSummaryResponse> GetProfitSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default);
    Task<InventoryValuationResponse> GetInventoryValuationAsync(CancellationToken cancellationToken = default);
}
