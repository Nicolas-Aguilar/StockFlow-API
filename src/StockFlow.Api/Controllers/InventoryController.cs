using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Inventory;
using StockFlow.Application.Interfaces;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("movements")]
    public async Task<ActionResult<InventoryMovementResponse>> CreateMovement([FromBody] CreateInventoryMovementRequest request, CancellationToken cancellationToken)
    {
        var response = await _inventoryService.CreateMovementAsync(request, cancellationToken);
        return Created(string.Empty, response);
    }

    [HttpGet("movements")]
    public async Task<ActionResult<PagedResponse<InventoryMovementResponse>>> GetMovements([FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetMovementsAsync(paginationQuery, cancellationToken));
    }

    [HttpGet("products/{productId:guid}/history")]
    public async Task<ActionResult<PagedResponse<InventoryMovementResponse>>> GetHistory(Guid productId, [FromQuery] PaginationQuery paginationQuery, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetProductHistoryAsync(productId, paginationQuery, cancellationToken));
    }
}
