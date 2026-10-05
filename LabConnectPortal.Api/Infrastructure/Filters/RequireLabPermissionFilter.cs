using LabConnectPortal.Api.Domain;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LabConnectPortal.Api.Infrastructure.Filters;

/// <summary>
/// Enforces CRUD permissions for UserLab users based on X-System-Entity header.
/// </summary>
public sealed class RequireLabPermissionFilter(ILabUserPermissionService permissionService) : IAsyncAuthorizationFilter
{
    private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Auth",
        "Syncfusion",
        "SepidRadisan",
        "Rasa",
        "TestECL",
    };

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return;

        if (endpoint?.Metadata.GetMetadata<SkipLabPermissionAttribute>() != null)
            return;

        if (endpoint?.Metadata.GetMetadata<SkipSystemEntityAttribute>() != null)
            return;

        var controllerName = context.RouteData.Values["controller"]?.ToString();
        if (!string.IsNullOrEmpty(controllerName))
        {
            if (ExemptControllers.Contains(controllerName))
                return;

            if (controllerName.StartsWith("Public", StringComparison.OrdinalIgnoreCase))
                return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            return;

        var userTypeClaim = context.HttpContext.User.FindFirst("userType")?.Value;
        if (!Enum.TryParse<UserType>(userTypeClaim, out var userType))
            return;

        if (userType is UserType.Administrator or UserType.AdminLab or UserType.User or UserType.Store or UserType.Admin)
            return;

        if (userType != UserType.UserLab)
            return;

        var entityId = controllerName?.Equals("Notification", StringComparison.OrdinalIgnoreCase) == true
            ? SystemEntity.Message
            : context.HttpContext.GetSystemEntity();
        if (!entityId.HasValue || !SystemEntity.IsLabPortalEntity(entityId.Value))
            return;

        var action = ResolveAction(context);
        if (action == null)
        {
            context.Result = Forbidden("نوع عملیات برای کنترل دسترسی تعریف نشده است.");
            return;
        }

        var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = Forbidden("کاربر احراز هویت نشده است.");
            return;
        }

        var allowed = await permissionService.HasPermissionAsync(userId, entityId.Value, action.Value);
        if (!allowed)
            context.Result = Forbidden("شما دسترسی لازم برای انجام این عملیات را ندارید.");
    }

    private static LabPermissionAction? ResolveAction(AuthorizationFilterContext context)
    {
        var overrideAction = context.ActionDescriptor.EndpointMetadata
            .OfType<LabPermissionActionAttribute>()
            .FirstOrDefault();
        if (overrideAction != null)
            return overrideAction.Action;

        var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var name)
            ? name
            : context.ActionDescriptor.DisplayName;

        if (string.IsNullOrWhiteSpace(actionName))
            return null;

        return InferActionFromName(actionName);
    }

    internal static LabPermissionAction? InferActionFromName(string actionName)
    {
        if (actionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Remove", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Clear", StringComparison.OrdinalIgnoreCase))
            return LabPermissionAction.Delete;

        if (actionName.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Add", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("ConfirmAdd", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("PostReception", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Checkout", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Submit", StringComparison.OrdinalIgnoreCase))
            return LabPermissionAction.Create;

        if (actionName.StartsWith("RejectCount", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("IsExist", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("ReceiveReporting", StringComparison.OrdinalIgnoreCase))
            return LabPermissionAction.View;

        if (actionName.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Toggle", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Save", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Upload", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Publish", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Unpublish", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Approve", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Reject", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Mark", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Close", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Reopen", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Complete", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Cancel", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("ConfirmPayment", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Reply", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Set", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Receiver", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Primary", StringComparison.OrdinalIgnoreCase))
            return LabPermissionAction.Update;

        if (actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("List", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Search", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Report", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Export", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Download", StringComparison.OrdinalIgnoreCase) ||
            actionName.StartsWith("Print", StringComparison.OrdinalIgnoreCase) ||
            actionName.Contains("Detail", StringComparison.OrdinalIgnoreCase) ||
            actionName.Contains("Reporting", StringComparison.OrdinalIgnoreCase) ||
            actionName.Contains("Printing", StringComparison.OrdinalIgnoreCase) ||
            actionName.Contains("Options", StringComparison.OrdinalIgnoreCase) ||
            actionName.Contains("Filters", StringComparison.OrdinalIgnoreCase))
            return LabPermissionAction.View;

        return null;
    }

    private static ObjectResult Forbidden(string message) =>
        new(OperationResult.Failure(message)) { StatusCode = StatusCodes.Status403Forbidden };
}
