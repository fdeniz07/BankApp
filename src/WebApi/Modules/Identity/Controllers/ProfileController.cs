using Application.Modules.Identity.Features.ProfileImages.Commands.Upload;
using Application.Modules.Identity.Features.Profiles.Queries.GetMine;
using Application.Modules.Identity.Features.StaffUsers.Commands.Update;
using Application.Services.Auth;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Identity.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class ProfileController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        GetMyProfileResponse response = await Mediator.Send(new GetMyProfileRequest());
        return Ok(response);
    }

    [HttpPut("staff")]
    public async Task<IActionResult> UpdateStaffProfile(
        [FromBody] UpdateStaffUserRequest request,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        request.Id = currentUserService.UserId.Value;
        UpdatedStaffUserResponse response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { detail = "Profile image file is required." });

        await using Stream stream = file.OpenReadStream();
        UploadProfileImageRequest request = new()
        {
            Content = stream,
            ContentType = file.ContentType,
            Length = file.Length
        };
        UploadProfileImageResponse response = await Mediator.Send(request);
        return Ok(response);
    }
}
