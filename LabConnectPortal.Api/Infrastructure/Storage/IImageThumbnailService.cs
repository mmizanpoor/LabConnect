namespace LabConnectPortal.Api.Infrastructure.Storage;

public readonly record struct PublicImagePayload(
    Stream? Stream,
    string? ContentType,
    string? ETag,
    DateTimeOffset? LastModified)
{
    public bool IsFound => Stream is not null && ContentType is not null;
}

public interface IImageThumbnailService
{
    /// <summary>
    /// Opens the original image, or a cached WebP resize when <paramref name="maxWidth"/> is set.
    /// When width is omitted/invalid, or resize fails, returns the original unchanged.
    /// </summary>
    Task<PublicImagePayload> OpenAsync(
        Func<Task<(Stream? Stream, string? ContentType)>> openOriginal,
        string relativePath,
        int? maxWidth);
}
