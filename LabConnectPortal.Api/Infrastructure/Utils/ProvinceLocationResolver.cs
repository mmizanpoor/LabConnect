using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Utils;

/// <summary>
/// Resolves an Iranian province from coordinates by nearest known centroid,
/// then matches the name against the Provinces table.
/// </summary>
public static class ProvinceLocationResolver
{
    // Approximate geographic centers for seeded Persian province names.
    private static readonly (string Name, double Lat, double Lng)[] Centroids =
    [
        ("آذربایجان شرقی", 37.9036, 46.2682),
        ("آذربایجان غربی", 37.5527, 45.0761),
        ("اردبیل", 38.4853, 47.8911),
        ("اصفهان", 32.6546, 51.6680),
        ("البرز", 35.8400, 50.9391),
        ("ایلام", 33.6374, 46.4226),
        ("بوشهر", 28.9234, 50.8203),
        ("تهران", 35.6892, 51.3890),
        ("چهارمحال و بختیاری", 32.3256, 50.8644),
        ("خراسان جنوبی", 32.8649, 59.2262),
        ("خراسان رضوی", 36.2972, 59.6067),
        ("خراسان شمالی", 37.4710, 57.1013),
        ("خوزستان", 31.4360, 49.0413),
        ("زنجان", 36.6769, 48.4963),
        ("سمنان", 35.5729, 53.3971),
        ("سیستان و بلوچستان", 29.4858, 60.8629),
        ("فارس", 29.5918, 52.5837),
        ("قزوین", 36.2797, 50.0049),
        ("قم", 34.6416, 50.8746),
        ("کردستان", 35.3115, 46.9963),
        ("کرمان", 30.2832, 57.0788),
        ("کرمانشاه", 34.3142, 47.0650),
        ("کهگیلویه و بویراحمد", 30.6500, 51.6000),
        ("گلستان", 37.2892, 55.1376),
        ("گیلان", 37.2808, 49.5921),
        ("لرستان", 33.5818, 48.3988),
        ("مازندران", 36.5659, 53.0586),
        ("مرکزی", 34.0917, 49.6892),
        ("هرمزگان", 27.1832, 56.2666),
        ("همدان", 34.7983, 48.5148),
        ("یزد", 31.8974, 54.3569),
    ];

    public static string? FindNearestProvinceName(double latitude, double longitude)
    {
        string? bestName = null;
        var bestDistance = double.MaxValue;

        foreach (var (name, lat, lng) in Centroids)
        {
            var distance = HaversineKm(latitude, longitude, lat, lng);
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            bestName = name;
        }

        // Reject clearly out-of-country pins (rough Iran diameter ~2000km; 450km buffer from nearest center).
        return bestDistance <= 450 ? bestName : null;
    }

    public static async Task<Province?> ResolveAsync(
        DbContext context,
        decimal? latitude,
        decimal? longitude,
        CancellationToken cancellationToken = default)
    {
        if (latitude is null || longitude is null) return null;

        var name = FindNearestProvinceName((double)latitude.Value, (double)longitude.Value);
        if (string.IsNullOrWhiteSpace(name)) return null;

        var normalized = Normalize(name);
        var provinces = await context.Set<Province>().AsNoTracking().ToListAsync(cancellationToken);
        return provinces.FirstOrDefault(p => Normalize(p.ProvinceName) == normalized)
            ?? provinces.FirstOrDefault(p => Normalize(p.ProvinceName).Contains(normalized)
                || normalized.Contains(Normalize(p.ProvinceName)));
    }

    private static string Normalize(string value)
        => value
            .Replace("ي", "ی", StringComparison.Ordinal)
            .Replace("ك", "ک", StringComparison.Ordinal)
            .Replace("‌", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .Trim();

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * r * Math.Asin(Math.Min(1, Math.Sqrt(a)));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
