using Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Create;
using Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Delete;
using Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Update;
using Application.Modules.Identity.Features.DecisionLetterTemplates.Queries.GetList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class DecisionLetterTemplatesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] string? kind = null)
    {
        IReadOnlyList<GetListDecisionLetterTemplateItemDto> response = await Mediator.Send(
            new GetListDecisionLetterTemplateRequest { Kind = kind });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDecisionLetterTemplateRequest request)
    {
        CreatedDecisionLetterTemplateResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateDecisionLetterTemplateRequest request)
    {
        UpdatedDecisionLetterTemplateResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        DeletedDecisionLetterTemplateResponse response = await Mediator.Send(
            new DeleteDecisionLetterTemplateRequest { Id = id });
        return Ok(response);
    }
}
