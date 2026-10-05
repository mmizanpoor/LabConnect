using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
[SkipSitePermission]
public class SiteUserController(
    ISiteUserService siteUserService,
    ISiteUserPermissionService permissionService) : ControllerBase
{
    [HttpGet("GetMembers")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> GetMembers()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await siteUserService.GetMembersAsync(userId);
    }

    [HttpPost("AddMember")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> AddMember([FromBody] AddSiteMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await siteUserService.AddMemberAsync(userId, command);
    }

    [HttpPost("ConfirmAddMember")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> ConfirmAddMember([FromBody] ConfirmAddSiteMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await siteUserService.ConfirmAddMemberAsync(userId, command);
    }

    [HttpPost("ToggleMemberActive")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> ToggleMemberActive([FromBody] ToggleSiteMemberActiveCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await siteUserService.ToggleMemberActiveAsync(userId, command);
    }

    [HttpPost("RemoveMember")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> RemoveMember([FromBody] RemoveSiteMemberCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await siteUserService.RemoveMemberAsync(userId, command);
    }

    [HttpGet("GetMemberPermissions")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> GetMemberPermissions(Guid memberId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions = await permissionService.GetMemberPermissionsAsync(userId, memberId);
        return OperationResult<List<SiteUserPermissionDto>>.Success(permissions);
    }

    [HttpPost("SetMemberPermissions")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> SetMemberPermissions([FromBody] SetSiteMemberPermissionsCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await permissionService.SetMemberPermissionsAsync(userId, command);
    }

    [HttpGet("GetMyPermissions")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    [SkipSystemEntity]
    public async Task<OperationResult> GetMyPermissions()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var permissions = await permissionService.GetForUserAsync(userId);
        return OperationResult<List<SiteUserPermissionDto>>.Success(permissions);
    }
}
