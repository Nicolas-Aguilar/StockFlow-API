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
    public async Task<ActionResult<PagedResponse<ProductResponse>>> LowStock([FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetLowStockProductsAsync(paginationQuery, cancellationToken));
    }

    [HttpGet("expiring-soon")]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> ExpiringSoon([FromQuery] PaginationQuery paginationQuery, [FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        return Ok(await _reportService.GetExpiringSoonProductsAsync(paginationQuery, days, cancellationToken));
    }

    [HttpGet("expired-products")]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> ExpiredProducts([FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetExpiredProductsAsync(paginationQuery, cancellationToken));
    }

    [HttpGet("top-selling-products")]
    public async Task<ActionResult<PagedResponse<TopSellingProductResponse>>> TopSellingProducts([FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _reportService.GetTopSellingProductsAsync(paginationQuery, cancellationToken));
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
