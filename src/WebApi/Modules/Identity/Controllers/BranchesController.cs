using Application.Modules.Identity.Features.Branches.Commands.Create;
using Application.Modules.Identity.Features.Branches.Commands.Update;
using Application.Modules.Identity.Features.Branches.Queries.GetList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class BranchesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] bool? isActive = true)
    {
        IReadOnlyList<GetListBranchListItemDto> response = await Mediator.Send(
            new GetListBranchRequest { IsActive = isActive });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchRequest request)
    {
        CreatedBranchResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateBranchRequest request)
    {
        UpdatedBranchResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
