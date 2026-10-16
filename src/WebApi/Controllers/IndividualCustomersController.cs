using Application.Features.IndividualCustomers.Commands.Create;
using Application.Features.IndividualCustomers.Commands.Delete;
using Application.Features.IndividualCustomers.Commands.Update;
using Application.Features.IndividualCustomers.Queries.GetById;
using Application.Features.IndividualCustomers.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class IndividualCustomersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] bool? isActive = null)
    {
        GetListIndividualCustomerRequest request = new()
        {
            PageRequest = pageRequest,
            IsActive = isActive
        };
        GetListResponse<GetListIndividualCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactiveList([FromQuery] PageRequest pageRequest)
    {
        GetListIndividualCustomerRequest request = new()
        {
            PageRequest = pageRequest,
            IsActive = false
        };
        GetListResponse<GetListIndividualCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedList([FromQuery] PageRequest pageRequest)
    {
        GetListIndividualCustomerRequest request = new()
        {
            PageRequest = pageRequest,
            OnlyDeleted = true
        };
        GetListResponse<GetListIndividualCustomerListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdIndividualCustomerRequest request = new() { Id = id };
        GetByIdIndividualCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateIndividualCustomerRequest request)
    {
        CreatedIndividualCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateIndividualCustomerRequest request)
    {
        UpdatedIndividualCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeleteIndividualCustomerRequest request = new() { Id = id };
        DeletedIndividualCustomerResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
