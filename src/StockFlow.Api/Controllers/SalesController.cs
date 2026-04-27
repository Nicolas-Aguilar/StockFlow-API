using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Sales;
using StockFlow.Application.Interfaces;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sales")]
public sealed class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var response = await _saleService.CreateAsync(request, cancellationToken);
        return Created($"/api/sales/{response.Id}", response);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SaleResponse>>> GetAll([FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _saleService.GetAllAsync(paginationQuery, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _saleService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<PagedResponse<SaleResponse>>> GetByDate([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _saleService.GetByDateRangeAsync(new DateRangeQuery { From = from, To = to }, paginationQuery, cancellationToken));
    }
}
