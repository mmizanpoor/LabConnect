using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LabConnectPortal.Api.Infrastructure.Storage;

public static class PublicImageResult
{
    public static async Task<IActionResult> FromAsync(
        ControllerBase controller,
        IImageThumbnailService thumbnails,
        Func<Task<(Stream? Stream, string? ContentType)>> openOriginal,
        string path,
        int? width = null,
        object? notFoundBody = null)
    {
        var payload = await thumbnails.OpenAsync(openOriginal, path, width);
        if (!payload.IsFound)
        {
            return notFoundBody is null
                ? controller.NotFound()
                : controller.NotFound(notFoundBody);
        }

        var responseHeaders = controller.Response.GetTypedHeaders();
        responseHeaders.CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.Zero,
            MustRevalidate = true,
        };

        EntityTagHeaderValue? etag = null;
        if (!string.IsNullOrWhiteSpace(payload.ETag))
        {
            etag = CreateEntityTag(payload.ETag);
            responseHeaders.ETag = etag;
        }

        if (payload.LastModified is { } lastModified)
            responseHeaders.LastModified = lastModified.UtcDateTime;

        var ifNoneMatch = controller.Request.GetTypedHeaders().IfNoneMatch;
        if (etag is not null && MatchesIfNoneMatch(ifNoneMatch, etag))
        {
            await payload.Stream!.DisposeAsync();
            return controller.StatusCode(StatusCodes.Status304NotModified);
        }

        return controller.File(payload.Stream!, payload.ContentType!);
    }

    /// <summary>
    /// Builds a strong entity tag from an opaque tag value (with or without surrounding quotes).
    /// </summary>
    internal static EntityTagHeaderValue CreateEntityTag(string tag)
    {
        var opaque = tag.Trim().Trim('"');
        return new EntityTagHeaderValue($"\"{opaque}\"");
    }

    /// <summary>
    /// RFC 7232 weak comparison for If-None-Match: ignore weakness, match opaque tags; support '*'.
    /// </summary>
    internal static bool MatchesIfNoneMatch(
        IList<EntityTagHeaderValue>? ifNoneMatch,
        EntityTagHeaderValue current)
    {
        if (ifNoneMatch is null || ifNoneMatch.Count == 0)
            return false;

        foreach (var candidate in ifNoneMatch)
        {
            if (candidate.Equals(EntityTagHeaderValue.Any))
                return true;

            if (string.Equals(candidate.Tag.Value, current.Tag.Value, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
