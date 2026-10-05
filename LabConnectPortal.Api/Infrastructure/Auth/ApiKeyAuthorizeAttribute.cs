using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace LabConnectPortal.Api.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAuthorizeAttribute : TypeFilterAttribute
{
    public ApiKeyAuthorizeAttribute(ApiKeyPermission permission = ApiKeyPermission.None)
        : base(typeof(ApiKeyAuthorizationFilter))
    {
        Arguments = [permission];
    }
}

public sealed class ApiKeyAuthorizationFilter(
    ApiKeyPermission permission,
    IApiKeyService apiKeyService) : IAsyncAuthorizationFilter
{
    public const string ContextItemKey = "ApiKeyAuthorization";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var result = await apiKeyService.AuthorizeAsync(
            ExtractApiKey(context.HttpContext.Request),
            permission);

        if (!result.Status || result.Data == null)
        {
            context.Result = new ObjectResult(OperationResult.Failure(
                result.Message ?? "شما دسترسی ندارید. برای دریافت دسترسی با شرکت تماس حاصل نمایید."))
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }

        context.HttpContext.Items[ContextItemKey] = result.Data;
    }

    private static string? ExtractApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Api-Key", out var xApiKey) &&
            !string.IsNullOrWhiteSpace(xApiKey))
            return xApiKey.ToString().Trim();

        if (request.Headers.TryGetValue("ApiKey", out var apiKey) &&
            !string.IsNullOrWhiteSpace(apiKey))
            return apiKey.ToString().Trim();

        var authorization = request.Headers[HeaderNames.Authorization].ToString().Trim();
        if (authorization.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
            return authorization["ApiKey ".Length..].Trim();

        return authorization.Contains(' ') || string.IsNullOrWhiteSpace(authorization)
            ? null
            : authorization;
    }
}
