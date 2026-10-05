using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using LabConnectPortal.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class UserController(IUserService userService, IUserProfileService userProfileService) : ControllerBase
{
    [HttpPost("GetUsers")]
    public async Task<OperationResult> GetUsers([FromBody] GetUsersQuery query)
        => await userService.GetUsersAsync(query);

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid id)
        => await userService.GetByIdAsync(id);

    [HttpPost("ToggleActive")]
    public async Task<OperationResult> ToggleActive([FromBody] ToggleActiveCommand command)
        => await userService.ToggleActiveAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid id)
        => await userService.DeleteAsync(id);

    [HttpGet("GetStats")]
    public async Task<OperationResult> GetStats()
        => await userService.GetStatsAsync();

    [HttpGet("GetResume")]
    public async Task<OperationResult> GetResume(Guid userId)
        => await userProfileService.GetMyResumeAsync(userId);

    [HttpGet("GetResumePhoto")]
    public async Task<IActionResult> GetResumePhoto(Guid userId)
    {
        var (stream, contentType) = await userProfileService.GetPhotoAsync(userId);
        if (stream == null || contentType == null)
            return NotFound();

        return File(stream, contentType);
    }

    [HttpGet("GetResumeFile")]
    public async Task<IActionResult> GetResumeFile(Guid userId)
    {
        var (stream, contentType, fileName) = await userProfileService.GetResumeAsync(userId);
        if (stream == null || contentType == null)
            return NotFound();

        return File(stream, contentType, fileName ?? "resume");
    }
}
