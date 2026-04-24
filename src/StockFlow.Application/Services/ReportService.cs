using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Reports;
using StockFlow.Application.Interfaces;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Services;

public sealed class ReportService : IReportService
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReportService(IProductRepository productRepository, ISaleRepository saleRepository, IUserContext userContext, IDateTimeProvider dateTimeProvider)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return (await _productRepository.GetLowStockAsync(_userContext.BusinessId, cancellationToken)).Select(product => product.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetExpiringSoonProductsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            throw new ValidationDomainException("The days parameter must be greater than zero.");
        }

        var limitDate = _dateTimeProvider.UtcNow.Date.AddDays(days);
        return (await _productRepository.GetExpiringSoonAsync(_userContext.BusinessId, limitDate, cancellationToken)).Select(product => product.ToResponse()).ToArray();
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetExpiredProductsAsync(CancellationToken cancellationToken = default)
    {
        return (await _productRepository.GetExpiredAsync(_userContext.BusinessId, _dateTimeProvider.UtcNow.Date, cancellationToken)).Select(product => product.ToResponse()).ToArray();
    }

    public Task<IReadOnlyCollection<TopSellingProductResponse>> GetTopSellingProductsAsync(CancellationToken cancellationToken = default)
    {
        return _saleRepository.GetTopSellingProductsAsync(_userContext.BusinessId, cancellationToken);
    }

    public async Task<SalesSummaryResponse> GetSalesSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(query);
        var sales = await _saleRepository.GetByDateRangeAsync(_userContext.BusinessId, query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), cancellationToken);
        return new SalesSummaryResponse(query.From, query.To, sales.Count, sales.Sum(sale => sale.Total));
    }

    public async Task<ProfitSummaryResponse> GetProfitSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(query);
        var sales = await _saleRepository.GetByDateRangeAsync(_userContext.BusinessId, query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), cancellationToken);
        return new ProfitSummaryResponse(query.From, query.To, sales.Count, sales.Sum(sale => sale.EstimatedProfit));
    }

    public async Task<InventoryValuationResponse> GetInventoryValuationAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(_userContext.BusinessId, null, cancellationToken);
        var purchaseValue = products.Sum(product => product.CurrentStock * product.PurchasePrice);
        var saleValue = products.Sum(product => product.CurrentStock * product.SalePrice);
        return new InventoryValuationResponse(products.Count, purchaseValue, saleValue, saleValue - purchaseValue);
    }

    private static void ValidateDateRange(DateRangeQuery query)
    {
        if (query.From > query.To)
        {
            throw new ValidationDomainException("The from date must be less than or equal to the to date.");
        }
    }
}
