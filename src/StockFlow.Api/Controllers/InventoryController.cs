using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<IReadOnlyCollection<InventoryMovementResponse>>> GetMovements(CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetMovementsAsync(cancellationToken));
    }

    [HttpGet("products/{productId:guid}/history")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryMovementResponse>>> GetHistory(Guid productId, CancellationToken cancellationToken)
    {
        return Ok(await _inventoryService.GetProductHistoryAsync(productId, cancellationToken));
    }
}
