using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.RegisterIndividual;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
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
}
