using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LabConnectPortal.Api.Infrastructure.Filters;

/// <summary>
/// Requires a known X-System-Entity header for authenticated frontend API calls.
/// </summary>
public sealed class RequireSystemEntityFilter : IAsyncAuthorizationFilter
{
    private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Auth",
        "Notification",
        "Syncfusion",
        "SepidRadisan",
        "Rasa",
        "TestECL",
        // Also called by external systems (not only the portal frontend).
        "Reception",
        "LabAgreement",
        "SecuritySmsReport",
        // Site-wide management: no per-entity scoping needed.
        "SiteService",
    };

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return Task.CompletedTask;

        if (endpoint?.Metadata.GetMetadata<SkipSystemEntityAttribute>() != null)
            return Task.CompletedTask;

        var controllerName = context.RouteData.Values["controller"]?.ToString();
        if (!string.IsNullOrEmpty(controllerName))
        {
            if (ExemptControllers.Contains(controllerName))
                return Task.CompletedTask;

            if (controllerName.StartsWith("Public", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;
        }

        var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated == true;
        var hasAuthorize = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>()?.Count > 0;
        if (!isAuthenticated && !hasAuthorize)
            return Task.CompletedTask;

        if (context.HttpContext.GetSystemEntity() == null)
        {
            context.Result = new ObjectResult(
                OperationResult.Failure("شناسه سامانه (SystemEntity) نامعتبر یا ارسال نشده است."))
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        return Task.CompletedTask;
    }
}
