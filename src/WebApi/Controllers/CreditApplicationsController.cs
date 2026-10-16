using Application.Features.CreditApplications.Commands.Approve;
using Application.Features.CreditApplications.Commands.Cancel;
using Application.Features.CreditApplications.Commands.Create;
using Application.Features.CreditApplications.Commands.Delete;
using Application.Features.CreditApplications.Commands.Reject;
using Application.Features.CreditApplications.Commands.Update;
using Application.Features.CreditApplications.Queries.GetById;
using Application.Features.CreditApplications.Queries.GetList;
using Application.Features.CreditApplications.Queries.GetPaymentPlan;
using Core.Application.Requests;
using Core.Application.Responses;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CreditApplicationsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] Guid? customerId,
        [FromQuery] string? status,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDesc = false)
    {
        CreditApplicationStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
            parsedStatus = CreditApplicationStatus.FromName(status, ignoreCase: true);

        GetListCreditApplicationRequest request = new()
        {
            PageRequest = pageRequest,
            CustomerId = customerId,
            Status = parsedStatus,
            SortBy = sortBy,
            SortDesc = sortDesc
        };
        GetListResponse<GetListCreditApplicationListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("payment-plan")]
    public async Task<IActionResult> GetPaymentPlan(
        [FromQuery] Guid? creditSubTypeId,
        [FromQuery] decimal amount,
        [FromQuery] int termMonths,
        [FromQuery] decimal? interestRate)
    {
        GetPaymentPlanRequest request = new()
        {
            CreditSubTypeId = creditSubTypeId,
            Amount = amount,
            TermMonths = termMonths,
            InterestRate = interestRate
        };
        GetPaymentPlanResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        GetByIdCreditApplicationRequest request = new() { Id = id };
        GetByIdCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateCreditApplicationRequest request)
    {
        CreatedCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateCreditApplicationRequest request)
    {
        UpdatedCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid id, 
        [FromBody] ApproveCreditApplicationRequest request)
    {
        request.Id = id;
        ApprovedCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(
        [FromRoute] Guid id, 
        [FromBody] RejectCreditApplicationRequest request)
    {
        request.Id = id;
        RejectedCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        [FromBody] CancelCreditApplicationRequest? request)
    {
        request ??= new CancelCreditApplicationRequest();
        request.Id = id;
        CancelledCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeleteCreditApplicationRequest request = new() { Id = id };
        DeletedCreditApplicationResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
