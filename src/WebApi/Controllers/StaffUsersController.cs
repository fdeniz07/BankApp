using Application.Features.StaffUsers.Commands.Create;
using Application.Features.StaffUsers.Commands.Update;
using Application.Features.StaffUsers.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class StaffUsersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] bool? isActive = null)
    {
        GetListStaffUserRequest request = new()
        {
            PageRequest = pageRequest,
            IsActive = isActive
        };
        GetListResponse<GetListStaffUserListItemDto> response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateStaffUserRequest request)
    {
        CreatedStaffUserResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateStaffUserRequest request)
    {
        UpdatedStaffUserResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
