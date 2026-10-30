using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Credit.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CreditDocumentsController : BaseController
{
    private readonly CreditDocumentsOptions _options;

    public CreditDocumentsController(IOptions<CreditDocumentsOptions> options)
    {
        _options = options.Value;
    }

    [HttpGet("config")]
    public IActionResult GetConfig()
    {
        return Ok(new CreditDocumentsConfigResponse(_options.SoftApproveGateEnabled));
    }

    public sealed record CreditDocumentsConfigResponse(bool SoftApproveGateEnabled);
}
