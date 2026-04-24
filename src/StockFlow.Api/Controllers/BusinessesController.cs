using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.DTOs.Businesses;
using StockFlow.Application.Interfaces;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/businesses")]
public sealed class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessesController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<BusinessResponse>> GetMe(CancellationToken cancellationToken)
    {
        return Ok(await _businessService.GetMyBusinessAsync(cancellationToken));
    }

    [HttpPut("me")]
    public async Task<ActionResult<BusinessResponse>> UpdateMe([FromBody] UpdateBusinessRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _businessService.UpdateMyBusinessAsync(request, cancellationToken));
    }
}
