using System.Globalization;
using System.Text.Json;

namespace LabConnectPortal.Api.Infrastructure.Audit;

public static class ActivityValueFormatter
{
    public static string? Format(object? value)
    {
        if (value is null)
            return null;

        return value switch
        {
            bool b => b ? "فعال" : "غیرفعال",
            DateTime dt => FormatDateTime(dt),
            DateTimeOffset dto => FormatDateTime(dto.UtcDateTime),
            Enum e => e.ToString(),
            decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
            double dbl => dbl.ToString("0.##", CultureInfo.InvariantCulture),
            float f => f.ToString("0.##", CultureInfo.InvariantCulture),
            Guid g => g.ToString(),
            string s => s,
            byte[] bytes => Convert.ToBase64String(bytes),
            _ when value.GetType().IsPrimitive => Convert.ToString(value, CultureInfo.InvariantCulture),
            _ => JsonSerializer.Serialize(value),
        };
    }

    private static string FormatDateTime(DateTime dt)
    {
        // Midnight values are treated as calendar dates (no timezone shift).
        if (dt is { Hour: 0, Minute: 0, Second: 0, Millisecond: 0 } && dt.Kind != DateTimeKind.Utc)
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var utc = dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime(),
        };
        return utc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " UTC";
    }
}
