using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Sales;

namespace StockFlow.Application.Interfaces;

public interface ISaleService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SaleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SaleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SaleResponse>> GetByDateRangeAsync(DateRangeQuery query, CancellationToken cancellationToken = default);
}
