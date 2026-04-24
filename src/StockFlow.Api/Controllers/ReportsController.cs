using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Reports;
using StockFlow.Application.Interfaces;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> LowStock(CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetLowStockProductsAsync(cancellationToken));
    }

    [HttpGet("expiring-soon")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> ExpiringSoon([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        return Ok(await _reportService.GetExpiringSoonProductsAsync(days, cancellationToken));
    }

    [HttpGet("expired-products")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> ExpiredProducts(CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetExpiredProductsAsync(cancellationToken));
    }

    [HttpGet("top-selling-products")]
    public async Task<ActionResult<IReadOnlyCollection<TopSellingProductResponse>>> TopSellingProducts(CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetTopSellingProductsAsync(cancellationToken));
    }

    [HttpGet("sales-summary")]
    public async Task<ActionResult<SalesSummaryResponse>> SalesSummary([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetSalesSummaryAsync(new DateRangeQuery { From = from, To = to }, cancellationToken));
    }

    [HttpGet("profit-summary")]
    public async Task<ActionResult<ProfitSummaryResponse>> ProfitSummary([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetProfitSummaryAsync(new DateRangeQuery { From = from, To = to }, cancellationToken));
    }

    [HttpGet("inventory-valuation")]
    public async Task<ActionResult<InventoryValuationResponse>> InventoryValuation(CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetInventoryValuationAsync(cancellationToken));
    }
}
