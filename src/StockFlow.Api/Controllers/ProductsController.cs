using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.Interfaces;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await _productService.CreateAsync(request, cancellationToken);
        return Created($"/api/products/{response.Id}", response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetAll([FromQuery] Guid? categoryId, CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetAllAsync(categoryId, cancellationToken));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> Search([FromQuery] string term, CancellationToken cancellationToken)
    {
        return Ok(await _productService.SearchAsync(term, cancellationToken));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> LowStock(CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetLowStockAsync(cancellationToken));
    }

    [HttpGet("expiring-soon")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> ExpiringSoon([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        return Ok(await _productService.GetExpiringSoonAsync(days, cancellationToken));
    }

    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> Expired(CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetExpiredAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _productService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _productService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ProductResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _productService.DeactivateAsync(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
