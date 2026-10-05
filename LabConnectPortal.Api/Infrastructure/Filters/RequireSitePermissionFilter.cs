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
/// Enforces CRUD permissions for UserType.Admin based on X-System-Entity header.
/// </summary>
public sealed class RequireSitePermissionFilter(ISiteUserPermissionService permissionService) : IAsyncAuthorizationFilter
{
    private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Auth",
        "Syncfusion",
        "SepidRadisan",
        "Rasa",
        "TestECL",
        "SiteUser",
    };

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return;

        if (endpoint?.Metadata.GetMetadata<SkipSitePermissionAttribute>() != null)
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

        if (userType == UserType.Administrator)
            return;

        if (userType != UserType.Admin)
            return;

        var entityId = controllerName?.Equals("Notification", StringComparison.OrdinalIgnoreCase) == true
            ? SystemEntity.Message
            : context.HttpContext.GetSystemEntity();
        if (!entityId.HasValue || !SystemEntity.IsSiteAdminEntity(entityId.Value))
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

        return RequireLabPermissionFilter.InferActionFromName(actionName);
    }

    private static ObjectResult Forbidden(string message) =>
        new(OperationResult.Failure(message)) { StatusCode = StatusCodes.Status403Forbidden };
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipSitePermissionAttribute : Attribute;
