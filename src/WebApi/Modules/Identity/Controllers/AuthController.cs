using Application.Modules.Identity.Features.Auth.Commands.ConfirmEmail;
using Application.Modules.Identity.Features.Auth.Commands.Login;
using Application.Modules.Identity.Features.Auth.Commands.Register;
using Application.Modules.Identity.Features.Auth.Commands.RegisterIndividual;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        LoggedResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        RegisteredResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("register-individual")]
    public async Task<IActionResult> RegisterIndividual([FromBody] RegisterIndividualRequest request)
    {
        RegisteredIndividualResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        ConfirmEmailResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
