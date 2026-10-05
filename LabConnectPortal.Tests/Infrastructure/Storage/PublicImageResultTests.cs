using LabConnectPortal.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LabConnectPortal.Tests.Infrastructure.Storage;

public class PublicImageResultTests
{
    private const string SampleTag = "abc123-1-2-full";
    private static readonly DateTimeOffset SampleLastModified =
        new(2026, 8, 29, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void MatchesIfNoneMatch_ReturnsFalse_WhenHeaderMissing()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        Assert.False(PublicImageResult.MatchesIfNoneMatch(null, current));
        Assert.False(PublicImageResult.MatchesIfNoneMatch([], current));
    }

    [Fact]
    public void MatchesIfNoneMatch_MatchesSingleEtag()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        var incoming = new List<EntityTagHeaderValue> { PublicImageResult.CreateEntityTag(SampleTag) };
        Assert.True(PublicImageResult.MatchesIfNoneMatch(incoming, current));
    }

    [Fact]
    public void MatchesIfNoneMatch_MatchesAmongMultipleEtags()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        var incoming = new List<EntityTagHeaderValue>
        {
            PublicImageResult.CreateEntityTag("other-1"),
            PublicImageResult.CreateEntityTag(SampleTag),
            PublicImageResult.CreateEntityTag("other-2"),
        };
        Assert.True(PublicImageResult.MatchesIfNoneMatch(incoming, current));
    }

    [Fact]
    public void MatchesIfNoneMatch_MatchesWeakEtag()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        var weak = new EntityTagHeaderValue($"\"{SampleTag}\"", isWeak: true);
        Assert.True(PublicImageResult.MatchesIfNoneMatch([weak], current));
    }

    [Fact]
    public void MatchesIfNoneMatch_MatchesStar()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        Assert.True(PublicImageResult.MatchesIfNoneMatch([EntityTagHeaderValue.Any], current));
    }

    [Fact]
    public void MatchesIfNoneMatch_ReturnsFalse_WhenDifferent()
    {
        var current = PublicImageResult.CreateEntityTag(SampleTag);
        var incoming = new List<EntityTagHeaderValue> { PublicImageResult.CreateEntityTag("different") };
        Assert.False(PublicImageResult.MatchesIfNoneMatch(incoming, current));
    }

    [Fact]
    public void CreateEntityTag_IsRfcQuotedStrongTag()
    {
        var etag = PublicImageResult.CreateEntityTag(SampleTag);
        Assert.False(etag.IsWeak);
        Assert.Equal($"\"{SampleTag}\"", etag.Tag.Value);
    }

    [Fact]
    public async Task FromAsync_Returns304_WhenIfNoneMatchEqualsEtag()
    {
        var (controller, _) = CreateController($"\"{SampleTag}\"");
        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, "image-bytes"u8.ToArray()),
            OpenUnused(),
            path: "uploads/a.png");

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status304NotModified, status.StatusCode);
        AssertResponseValidators(controller);
    }

    [Fact]
    public async Task FromAsync_Returns200WithBody_WhenIfNoneMatchDiffers()
    {
        var (controller, _) = CreateController("\"other-tag\"");
        var body = "image-bytes"u8.ToArray();
        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, body),
            OpenUnused(),
            path: "uploads/a.png");

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", file.ContentType);
        using var ms = new MemoryStream();
        await file.FileStream.CopyToAsync(ms);
        Assert.Equal(body, ms.ToArray());
        AssertResponseValidators(controller);
    }

    [Fact]
    public async Task FromAsync_Returns200_WhenIfNoneMatchAbsent()
    {
        var (controller, _) = CreateController(ifNoneMatch: null);
        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, "image-bytes"u8.ToArray()),
            OpenUnused(),
            path: "uploads/a.png");

        Assert.IsType<FileStreamResult>(result);
        AssertResponseValidators(controller);
    }

    [Fact]
    public async Task FromAsync_Returns304_WhenIfNoneMatchContainsMatchingAmongMany()
    {
        var (controller, http) = CreateController(null);
        http.Request.Headers.IfNoneMatch = "\"nope\", W/\"abc123-1-2-full\", \"also-nope\"";

        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, "image-bytes"u8.ToArray()),
            OpenUnused(),
            path: "uploads/a.png");

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status304NotModified, status.StatusCode);
    }

    [Fact]
    public async Task FromAsync_Returns304_WhenIfNoneMatchIsStar()
    {
        var (controller, _) = CreateController("*");
        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, "image-bytes"u8.ToArray()),
            OpenUnused(),
            path: "uploads/a.png");

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status304NotModified, status.StatusCode);
    }

    [Fact]
    public async Task FromAsync_Returns304_WhenIfNoneMatchIsWeakForm()
    {
        var (controller, _) = CreateController($"W/\"{SampleTag}\"");
        var result = await PublicImageResult.FromAsync(
            controller,
            CreateThumbnails(SampleTag, "image-bytes"u8.ToArray()),
            OpenUnused(),
            path: "uploads/a.png");

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status304NotModified, status.StatusCode);
    }

    private static void AssertResponseValidators(ControllerBase controller)
    {
        var headers = controller.Response.GetTypedHeaders();
        Assert.NotNull(headers.ETag);
        Assert.Equal($"\"{SampleTag}\"", headers.ETag!.Tag.Value);
        Assert.False(headers.ETag.IsWeak);
        Assert.Equal(SampleLastModified.UtcDateTime, headers.LastModified);
        Assert.NotNull(headers.CacheControl);
        Assert.True(headers.CacheControl!.Public);
        Assert.Equal(TimeSpan.Zero, headers.CacheControl.MaxAge);
        Assert.True(headers.CacheControl.MustRevalidate);
    }

    private static (TestController Controller, DefaultHttpContext Http) CreateController(string? ifNoneMatch)
    {
        var http = new DefaultHttpContext();
        if (ifNoneMatch is not null)
            http.Request.Headers.IfNoneMatch = ifNoneMatch;

        var controller = new TestController
        {
            ControllerContext = new ControllerContext { HttpContext = http },
        };
        return (controller, http);
    }

    private static FakeThumbnailService CreateThumbnails(string etag, byte[] body) =>
        new(
            new PublicImagePayload(
                new MemoryStream(body),
                "image/png",
                etag,
                SampleLastModified));

    private static Func<Task<(Stream? Stream, string? ContentType)>> OpenUnused() =>
        () => Task.FromResult<(Stream?, string?)>((new MemoryStream(), "image/png"));

    private sealed class TestController : ControllerBase;

    private sealed class FakeThumbnailService(PublicImagePayload payload) : IImageThumbnailService
    {
        public Task<PublicImagePayload> OpenAsync(
            Func<Task<(Stream? Stream, string? ContentType)>> openOriginal,
            string relativePath,
            int? maxWidth) => Task.FromResult(payload);
    }
}
