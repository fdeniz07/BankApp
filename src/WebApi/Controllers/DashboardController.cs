using Application.Features.Dashboard.Queries.GetSummary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class DashboardController : BaseController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        GetDashboardSummaryResponse response = await Mediator.Send(new GetDashboardSummaryRequest());
        return Ok(response);
    }
}
