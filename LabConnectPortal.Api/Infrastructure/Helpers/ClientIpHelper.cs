using System.Security.Cryptography;
using System.Text;

namespace LabConnectPortal.Api.Infrastructure.Helpers;

public static class ClientIpHelper
{
    public static string? ResolveClientIp(HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIp = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstIp))
                return firstIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    public static string HashIp(string ip, string pepper)
    {
        var input = $"{pepper}:{ip.Trim()}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
