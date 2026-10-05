using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ActivityLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class ActivityLogController(IActivityLogService activityLogService) : ControllerBase
{
    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.Store, UserType.AdminLab, UserType.UserLab)]
    public async Task<OperationResult> GetAll([FromBody] GetActivityLogsQuery query)
        => await activityLogService.GetGroupedAsync(GetUserId(), query);

    [HttpPost("GetByRecord")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.Store, UserType.AdminLab, UserType.UserLab)]
    public async Task<OperationResult> GetByRecord([FromBody] GetActivityLogsByRecordQuery query)
        => await activityLogService.GetByRecordAsync(GetUserId(), query);

    [HttpGet("GetBatch")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.Store, UserType.AdminLab, UserType.UserLab)]
    public async Task<OperationResult> GetBatch(Guid batchId)
        => await activityLogService.GetBatchAsync(GetUserId(), batchId);

    [HttpGet("GetCenterUsers")]
    [AuthorizeUserTypes(UserType.Store, UserType.AdminLab, UserType.UserLab)]
    public async Task<OperationResult> GetCenterUsers()
        => await activityLogService.GetCenterUsersAsync(GetUserId());

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
