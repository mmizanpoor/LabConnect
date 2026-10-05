using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LabConnectPortal.Api.Infrastructure.Storage;

public sealed class ImageThumbnailService(
    IWebHostEnvironment environment,
    IOptions<FileStorageSettings> options,
    ILogger<ImageThumbnailService> logger) : IImageThumbnailService
{
    internal const int MaxEdgeLength = 8192;
    internal const long MaxPixelCount = 32_000_000L;

    private static readonly int[] AllowedWidths = [160, 240, 320, 480, 640, 960, 1280, 1600, 1920, 2880];
    private static readonly SemaphoreSlim ResizeGate = new(2, 2);

    private readonly FileStorageSettings _settings = options.Value;

    public async Task<PublicImagePayload> OpenAsync(
        Func<Task<(Stream? Stream, string? ContentType)>> openOriginal,
        string relativePath,
        int? maxWidth)
    {
        var version = TryGetContentVersion(relativePath);
        var width = NormalizeWidth(maxWidth);
        var etag = BuildETag(relativePath, version, width);

        if (width is null || IsSvg(relativePath))
        {
            var (stream, contentType) = await openOriginal();
            return new PublicImagePayload(stream, contentType, etag, version?.LastModified);
        }

        var cachePath = GetCacheAbsolutePath(relativePath, width.Value, version);
        if (File.Exists(cachePath))
            return new PublicImagePayload(OpenReadShare(cachePath), "image/webp", etag, version?.LastModified);

        await ResizeGate.WaitAsync();
        try
        {
            if (File.Exists(cachePath))
                return new PublicImagePayload(OpenReadShare(cachePath), "image/webp", etag, version?.LastModified);

            var (originalStream, contentType) = await openOriginal();
            if (originalStream is null || contentType is null)
                return new PublicImagePayload(null, null, null, null);

            try
            {
                if (originalStream.CanSeek)
                {
                    if (originalStream.Length > _settings.MaxFileSizeBytes)
                    {
                        logger.LogWarning(
                            "Skipping thumbnail for {Path}: source length {Length} exceeds limit {Limit}",
                            relativePath,
                            originalStream.Length,
                            _settings.MaxFileSizeBytes);
                        originalStream.Position = 0;
                        return new PublicImagePayload(originalStream, contentType, etag, version?.LastModified);
                    }

                    originalStream.Position = 0;
                }

                if (originalStream.CanSeek)
                {
                    var identity = await Image.IdentifyAsync(originalStream);
                    originalStream.Position = 0;
                    if (identity is not null && IsUnsafeDimension(identity.Width, identity.Height))
                    {
                        logger.LogError(
                            "Skipping thumbnail for {Path}: image dimensions {Width}x{Height} exceed safe limits (max edge {MaxEdge}, max pixels {MaxPixels})",
                            relativePath,
                            identity.Width,
                            identity.Height,
                            MaxEdgeLength,
                            MaxPixelCount);
                        return new PublicImagePayload(originalStream, contentType, etag, version?.LastModified);
                    }
                }

                await using (originalStream)
                {
                    using var image = await Image.LoadAsync(originalStream);
                    if (IsUnsafeDimension(image.Width, image.Height))
                    {
                        logger.LogError(
                            "Skipping thumbnail for {Path}: image dimensions {Width}x{Height} exceed safe limits (max edge {MaxEdge}, max pixels {MaxPixels})",
                            relativePath,
                            image.Width,
                            image.Height,
                            MaxEdgeLength,
                            MaxPixelCount);
                        return await OpenOriginalPayloadAsync(openOriginal, etag, version);
                    }

                    if (image.Width <= width.Value)
                        return await OpenOriginalPayloadAsync(openOriginal, etag, version);

                    image.Mutate(ctx => ctx.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(width.Value, 0),
                    }));

                    var directory = Path.GetDirectoryName(cachePath)!;
                    Directory.CreateDirectory(directory);

                    var tempPath = Path.Combine(
                        directory,
                        $"{Path.GetFileNameWithoutExtension(cachePath)}_{Guid.NewGuid():N}.tmp");

                    try
                    {
                        await image.SaveAsWebpAsync(
                            tempPath,
                            new WebpEncoder { Quality = width.Value >= 1280 ? 88 : 78 });
                        File.Move(tempPath, cachePath, overwrite: true);
                    }
                    finally
                    {
                        TryDelete(tempPath);
                    }
                }

                return new PublicImagePayload(OpenReadShare(cachePath), "image/webp", etag, version?.LastModified);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate thumbnail for {Path} at width {Width}", relativePath, width);
                TryDelete(cachePath);
                return await OpenOriginalPayloadAsync(openOriginal, etag, version);
            }
        }
        finally
        {
            ResizeGate.Release();
        }
    }

    private static async Task<PublicImagePayload> OpenOriginalPayloadAsync(
        Func<Task<(Stream? Stream, string? ContentType)>> openOriginal,
        string? etag,
        ContentVersion? version)
    {
        var (stream, contentType) = await openOriginal();
        return new PublicImagePayload(stream, contentType, etag, version?.LastModified);
    }

    internal static bool IsUnsafeDimension(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return true;

        if (width > MaxEdgeLength || height > MaxEdgeLength)
            return true;

        return (long)width * height > MaxPixelCount;
    }

    private static int? NormalizeWidth(int? maxWidth)
    {
        if (maxWidth is null or <= 0)
            return null;

        foreach (var allowed in AllowedWidths)
        {
            if (maxWidth.Value <= allowed)
                return allowed;
        }

        return AllowedWidths[^1];
    }

    private static bool IsSvg(string relativePath)
        => Path.GetExtension(relativePath).Equals(".svg", StringComparison.OrdinalIgnoreCase);

    private ContentVersion? TryGetContentVersion(string relativePath)
    {
        if (!TryResolveAbsolutePath(relativePath, out var absolutePath) || !File.Exists(absolutePath))
            return null;

        var info = new FileInfo(absolutePath);
        return new ContentVersion(
            info.LastWriteTimeUtc.Ticks,
            info.Length,
            new DateTimeOffset(DateTime.SpecifyKind(info.LastWriteTimeUtc, DateTimeKind.Utc)));
    }

    private string GetCacheAbsolutePath(string relativePath, int width, ContentVersion? version)
    {
        var pathHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(relativePath.Replace('\\', '/').ToLowerInvariant())))
            .ToLowerInvariant()[..24];
        var versionToken = version is null
            ? "unknown"
            : $"{version.LastWriteTicks:x}_{version.Length:x}";
        var relativeCache = Path.Combine(
                _settings.StorageRoot,
                ".thumbs",
                $"{pathHash}_w{width}_v{versionToken}.webp")
            .Replace('\\', '/');
        return Path.Combine(
            environment.ContentRootPath,
            relativeCache.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string? BuildETag(string relativePath, ContentVersion? version, int? width)
    {
        if (version is null)
            return null;

        var pathHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(relativePath.Replace('\\', '/').ToLowerInvariant())))
            .ToLowerInvariant()[..16];
        var widthPart = width?.ToString(CultureInfo.InvariantCulture) ?? "full";
        // Opaque tag only — quoting is applied via EntityTagHeaderValue in PublicImageResult.
        return $"{pathHash}-{version.LastWriteTicks:x}-{version.Length:x}-{widthPart}";
    }

    private bool TryResolveAbsolutePath(string relativePath, out string absolutePath)
    {
        absolutePath = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath))
            return false;

        var normalized = relativePath.Replace('\\', '/').Trim();
        if (normalized.StartsWith('/') || normalized.Contains("..", StringComparison.Ordinal))
            return false;

        var candidate = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var storageRoot = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, _settings.StorageRoot));

        var rootPrefix = storageRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(candidate, storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        absolutePath = candidate;
        return true;
    }

    private static FileStream OpenReadShare(string path)
        => new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup of failed temp/cache files.
        }
    }

    private sealed record ContentVersion(long LastWriteTicks, long Length, DateTimeOffset LastModified);
}
