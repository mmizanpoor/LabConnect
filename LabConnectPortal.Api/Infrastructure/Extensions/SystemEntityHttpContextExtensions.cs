using LabConnectPortal.Api.Infrastructure.Middleware;

namespace LabConnectPortal.Api.Infrastructure.Extensions;

public static class SystemEntityHttpContextExtensions
{
    /// <summary>
    /// Returns the SystemEntity GUID from the current request when the X-System-Entity header was valid.
    /// </summary>
    public static Guid? GetSystemEntity(this HttpContext context)
    {
        if (context.Items.TryGetValue(SystemEntityMiddleware.HttpContextItemKey, out var value)
            && value is Guid guid)
        {
            return guid;
        }

        return null;
    }
}
