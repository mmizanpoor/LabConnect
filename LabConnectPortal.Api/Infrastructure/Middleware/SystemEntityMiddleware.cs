using LabConnectPortal.Api.Domain;

namespace LabConnectPortal.Api.Infrastructure.Middleware;

public class SystemEntityMiddleware(RequestDelegate next)
{
    public const string HttpContextItemKey = "SystemEntity";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(SystemEntity.HeaderName, out var values))
        {
            var raw = values.FirstOrDefault();
            if (Guid.TryParse(raw, out var entityId) && SystemEntity.IsKnown(entityId))
            {
                context.Items[HttpContextItemKey] = entityId;
            }
        }

        await next(context);
    }
}
