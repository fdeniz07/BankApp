using Application.Modules.Credit.Features.CreditSubTypes.Commands.Create;
using Application.Modules.Credit.Features.CreditSubTypes.Commands.Delete;
using Application.Modules.Credit.Features.CreditSubTypes.Commands.Update;
using Application.Modules.Credit.Features.CreditSubTypes.Queries.GetById;
using Application.Modules.Credit.Features.CreditSubTypes.Queries.GetList;
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
public class CreditSubTypesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] Guid? creditTypeId,
        [FromQuery] string? locale,
        [FromQuery] string? category)
    {
        GetListCreditSubTypeRequest request = new()
        {
            PageRequest = pageRequest,
            CreditTypeId = creditTypeId,
            Locale = locale,
            Category = category
        };
        GetListResponse<GetListCreditSubTypeListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdCreditSubTypeRequest request = new() { Id = id };
        GetByIdCreditSubTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCreditSubTypeRequest request)
    {
        CreatedCreditSubTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCreditSubTypeRequest request)
    {
        UpdatedCreditSubTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeleteCreditSubTypeRequest request = new() { Id = id };
        DeletedCreditSubTypeResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
