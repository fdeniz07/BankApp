using Application.Modules.Credit.Features.CreditTypes.Commands.Create;
using Application.Modules.Credit.Features.CreditTypes.Commands.Delete;
using Application.Modules.Credit.Features.CreditTypes.Commands.SetActive;
using Application.Modules.Credit.Features.CreditTypes.Commands.Update;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetById;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using Application.Services.Search;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Credit.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CreditTypesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] string? locale,
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? currencyCode,
        [FromQuery] string? riskLevel,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false)
    {
        GetListCreditTypeRequest request = new()
        {
            PageRequest = pageRequest,
            Locale = locale,
            Category = category,
            Search = search,
            CurrencyCode = currencyCode,
            RiskLevel = riskLevel,
            IsActive = isActive,
            SortBy = sortBy,
            SortDesc = sortDesc
        };
        GetListResponse<GetListCreditTypeListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("facets")]
    public async Task<IActionResult> GetFacets(
        [FromQuery] string? search,
        [FromQuery] string? locale,
        [FromQuery] string? category,
        [FromQuery] string? currencyCode,
        [FromQuery] string? riskLevel,
        [FromQuery] bool? isActive)
    {
        CreditTypeFacetsResponse response = await Mediator.Send(new GetCreditTypeFacetsRequest
        {
            Search = search,
            Locale = locale,
            Category = category,
            CurrencyCode = currencyCode,
            RiskLevel = riskLevel,
            IsActive = isActive
        });
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdCreditTypeRequest request = new() { Id = id };
        GetByIdCreditTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCreditTypeRequest request)
    {
        CreatedCreditTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCreditTypeRequest request)
    {
        UpdatedCreditTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut("{id}/active")]
    public async Task<IActionResult> SetActive([FromRoute] Guid id, [FromBody] SetCreditTypeActiveBody body)
    {
        SetCreditTypeActiveResponse response = await Mediator.Send(
            new SetCreditTypeActiveRequest { Id = id, IsActive = body.IsActive });
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeleteCreditTypeRequest request = new() { Id = id };
        DeletedCreditTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}

public sealed class SetCreditTypeActiveBody
{
    public bool IsActive { get; set; }
}
