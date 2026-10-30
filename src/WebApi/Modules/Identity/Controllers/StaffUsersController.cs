using Application.Modules.Identity.Features.StaffUsers.Commands.Create;
using Application.Modules.Identity.Features.StaffUsers.Commands.Update;
using Application.Modules.Identity.Features.StaffUsers.Queries.GetList;
using Core.Application.Requests;
using Core.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class StaffUsersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PageRequest pageRequest,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? branchId = null)
    {
        GetListStaffUserRequest request = new()
        {
            PageRequest = pageRequest,
            IsActive = isActive,
            BranchId = branchId
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
