using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Filters;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet("GetMyNotifications")]
    public async Task<OperationResult> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await notificationService.GetMyNotificationsAsync(userId);
    }

    [HttpGet("GetUnreadCount")]
    [SkipLabPermission]
    public async Task<OperationResult> GetUnreadCount()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await notificationService.GetUnreadCountAsync(userId);
    }

    [HttpPost("MarkAsRead")]
    public async Task<OperationResult> MarkAsRead([FromBody] MarkNotificationReadCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await notificationService.MarkAsReadAsync(userId, command.Id);
    }

    [HttpPost("SendExternalNotification")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> SendExternalNotification([FromBody] SendExternalNotificationCommand command)
        => await notificationService.SendExternalNotificationAsync(command.Notification, command.TargetUserIds);
}
