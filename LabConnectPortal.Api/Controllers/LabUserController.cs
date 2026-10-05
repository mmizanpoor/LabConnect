using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class LabUserController(
    ILabUserService labUserService,
    ILabUserPermissionService permissionService) : ControllerBase
{
    [HttpGet("GetMembers")]
    public async Task<OperationResult> GetMembers()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await labUserService.GetMembersAsync(userId);
    }

    [HttpPost("AddMember")]
    public async Task<OperationResult> AddMember([FromBody] AddLabMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await labUserService.AddMemberAsync(userId, command);
    }

    [HttpPost("ConfirmAddMember")]
    public async Task<OperationResult> ConfirmAddMember([FromBody] ConfirmAddLabMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await labUserService.ConfirmAddMemberAsync(userId, command);
    }

    [HttpPost("ToggleMemberActive")]
    public async Task<OperationResult> ToggleMemberActive([FromBody] ToggleLabMemberActiveCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await labUserService.ToggleMemberActiveAsync(userId, command);
    }

    [HttpPost("RemoveMember")]
    public async Task<OperationResult> RemoveMember([FromBody] RemoveLabMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await labUserService.RemoveMemberAsync(userId, command);
    }

    [HttpGet("GetMemberPermissions")]
    public async Task<OperationResult> GetMemberPermissions(Guid memberId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions = await permissionService.GetMemberPermissionsAsync(userId, memberId);
        return OperationResult<List<LabUserPermissionDto>>.Success(permissions);
    }

    [HttpPost("SetMemberPermissions")]
    public async Task<OperationResult> SetMemberPermissions([FromBody] SetLabMemberPermissionsCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await permissionService.SetMemberPermissionsAsync(userId, command);
    }

    [HttpGet("GetMyPermissions")]
    [SkipLabPermission]
    public async Task<OperationResult> GetMyPermissions()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions = await permissionService.GetForUserAsync(userId);
        return OperationResult<List<LabUserPermissionDto>>.Success(permissions);
    }
}
