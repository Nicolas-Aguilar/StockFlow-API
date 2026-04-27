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

    public async Task<PagedResponse<ProductResponse>> GetLowStockProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var currentDateUtc = _dateTimeProvider.UtcNow;
        return (await _productRepository.GetLowStockPagedAsync(_userContext.BusinessId, paginationQuery, cancellationToken)).ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<ProductResponse>> GetExpiringSoonProductsAsync(PaginationQuery paginationQuery, int days = 30, CancellationToken cancellationToken = default)
    {
        if (days <= 0)
        {
            throw new ValidationDomainException("The days parameter must be greater than zero.");
        }

        var currentDateUtc = _dateTimeProvider.UtcNow;
        var limitDate = currentDateUtc.Date.AddDays(days);
        return (await _productRepository.GetExpiringSoonPagedAsync(_userContext.BusinessId, currentDateUtc.Date, limitDate, paginationQuery, cancellationToken)).ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<ProductResponse>> GetExpiredProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var currentDateUtc = _dateTimeProvider.UtcNow;
        return (await _productRepository.GetExpiredPagedAsync(_userContext.BusinessId, currentDateUtc.Date, paginationQuery, cancellationToken)).ToPagedResponse(product => product.ToResponse(currentDateUtc));
    }

    public async Task<PagedResponse<TopSellingProductResponse>> GetTopSellingProductsAsync(PaginationQuery paginationQuery, CancellationToken cancellationToken = default)
    {
        var products = await _saleRepository.GetTopSellingProductsPagedAsync(_userContext.BusinessId, paginationQuery, cancellationToken);
        return products.ToPagedResponse(product => product);
    }

    public async Task<SalesSummaryResponse> GetSalesSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(query);
        var totals = await _saleRepository.GetSalesTotalsByDateRangeAsync(_userContext.BusinessId, query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), cancellationToken);
        return new SalesSummaryResponse(query.From, query.To, totals.TotalSales, totals.TotalAmount);
    }

    public async Task<ProfitSummaryResponse> GetProfitSummaryAsync(DateRangeQuery query, CancellationToken cancellationToken = default)
    {
        ValidateDateRange(query);
        var totals = await _saleRepository.GetProfitTotalsByDateRangeAsync(_userContext.BusinessId, query.From.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), query.To.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), cancellationToken);
        return new ProfitSummaryResponse(query.From, query.To, totals.TotalSales, totals.EstimatedProfit);
    }

    public async Task<InventoryValuationResponse> GetInventoryValuationAsync(CancellationToken cancellationToken = default)
    {
        var totals = await _productRepository.GetInventoryValuationTotalsAsync(_userContext.BusinessId, cancellationToken);
        return new InventoryValuationResponse(totals.TotalProducts, totals.PurchaseValue, totals.PotentialSaleValue, totals.PotentialSaleValue - totals.PurchaseValue);
    }

    private static void ValidateDateRange(DateRangeQuery query)
    {
        if (query.From > query.To)
        {
            throw new ValidationDomainException("The from date must be less than or equal to the to date.");
        }
    }
}
