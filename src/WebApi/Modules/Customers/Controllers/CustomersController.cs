using Application.Modules.Customers.Features.Customers.Queries.GetFacets;
using Application.Services.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Customers.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CustomersController : BaseController
{
    [HttpGet("facets")]
    public async Task<IActionResult> GetFacets(
        [FromQuery] string kind = CustomerSearchKinds.Individual,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool onlyDeleted = false,
        [FromQuery] Guid? branchId = null)
    {
        CustomerFacetsResponse response = await Mediator.Send(new GetCustomerFacetsRequest
        {
            Kind = kind,
            Search = search,
            IsActive = isActive,
            OnlyDeleted = onlyDeleted,
            BranchId = branchId
        });
        return Ok(response);
    }
}
