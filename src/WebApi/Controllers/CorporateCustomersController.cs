using Application.Features.CorporateCustomers.Commands.Create;
using Application.Features.CorporateCustomers.Commands.Delete;
using Application.Features.CorporateCustomers.Commands.Update;
using Application.Features.CorporateCustomers.Queries.GetById;
using Application.Features.CorporateCustomers.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CorporateCustomersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PageRequest pageRequest)
    {
        GetListCorporateCustomerRequest request = new() { PageRequest = pageRequest };
        GetListResponse<GetListCorporateCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactiveList([FromQuery] PageRequest pageRequest)
    {
        GetListCorporateCustomerRequest request = new()
        {
            PageRequest = pageRequest,
            IsActive = false
        };
        GetListResponse<GetListCorporateCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedList([FromQuery] PageRequest pageRequest)
    {
        GetListCorporateCustomerRequest request = new()
        {
            PageRequest = pageRequest,
            OnlyDeleted = true
        };
        GetListResponse<GetListCorporateCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdCorporateCustomerRequest request = new() { Id = id };
        GetByIdCorporateCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCorporateCustomerRequest request)
    {
        CreatedCorporateCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCorporateCustomerRequest request)
    {
        UpdatedCorporateCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeleteCorporateCustomerRequest request = new() { Id = id };
        DeletedCorporateCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
