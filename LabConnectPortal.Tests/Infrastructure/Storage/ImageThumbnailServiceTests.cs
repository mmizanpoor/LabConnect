using LabConnectPortal.Api.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace LabConnectPortal.Tests.Infrastructure.Storage;

public class ImageThumbnailServiceTests : IDisposable
{
    private readonly string _root;
    private readonly ImageThumbnailService _service;

    public ImageThumbnailServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "lcp-thumb-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "uploads"));

        var env = new TestHostEnvironment(_root);
        var options = Options.Create(new FileStorageSettings
        {
            StorageRoot = "uploads",
            MaxFileSizeBytes = 5 * 1024 * 1024,
        });
        _service = new ImageThumbnailService(env, options, NullLogger<ImageThumbnailService>.Instance);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // Best-effort cleanup of temp test files.
        }
    }

    [Theory]
    [InlineData(9000, 100)]
    [InlineData(100, 9000)]
    [InlineData(6000, 6000)]
    public void IsUnsafeDimension_RejectsOversizedImages(int width, int height)
    {
        Assert.True(ImageThumbnailService.IsUnsafeDimension(width, height));
    }

    [Fact]
    public void IsUnsafeDimension_AllowsSafeImages()
    {
        Assert.False(ImageThumbnailService.IsUnsafeDimension(1920, 1080));
        Assert.False(ImageThumbnailService.IsUnsafeDimension(8192, 3900));
    }

    [Fact]
    public async Task OpenAsync_WithoutWidth_ReturnsOriginalUnchanged()
    {
        var relativePath = await SavePngAsync("uploads/products/1/original.png", 640, 480, Color.Blue);
        var originalBytes = await File.ReadAllBytesAsync(Path.Combine(_root, relativePath));

        var payload = await _service.OpenAsync(
            () => OpenFileAsync(relativePath, "image/png"),
            relativePath,
            maxWidth: null);

        Assert.True(payload.IsFound);
        Assert.Equal("image/png", payload.ContentType);
        Assert.NotNull(payload.ETag);

        await using (payload.Stream)
        {
            using var ms = new MemoryStream();
            await payload.Stream!.CopyToAsync(ms);
            Assert.Equal(originalBytes, ms.ToArray());
        }
    }

    [Fact]
    public async Task OpenAsync_WithHugeDimensions_SkipsResize_AndReturnsOriginal()
    {
        // Wide image stays modest on disk/RAM while exceeding MaxEdgeLength (8192).
        var relativePath = await SavePngAsync("uploads/products/1/bomb.png", 8500, 120, Color.Red);
        var info = new FileInfo(Path.Combine(_root, relativePath));
        Assert.True(info.Length < 5 * 1024 * 1024, "Test fixture must stay under MaxFileSizeBytes");

        var originalBytes = await File.ReadAllBytesAsync(info.FullName);

        var payload = await _service.OpenAsync(
            () => OpenFileAsync(relativePath, "image/png"),
            relativePath,
            maxWidth: 320);

        Assert.True(payload.IsFound);
        Assert.Equal("image/png", payload.ContentType);

        await using (payload.Stream)
        {
            using var ms = new MemoryStream();
            await payload.Stream!.CopyToAsync(ms);
            Assert.Equal(originalBytes, ms.ToArray());
        }

        var thumbsDir = Path.Combine(_root, "uploads", ".thumbs");
        if (Directory.Exists(thumbsDir))
            Assert.Empty(Directory.GetFiles(thumbsDir, "*.webp"));
    }

    [Fact]
    public async Task OpenAsync_WhenFileReplacedAtSamePath_ProducesNewThumbnail()
    {
        var relativePath = "uploads/products/1/card.png";
        await SavePngAsync(relativePath, 800, 800, Color.Green);

        var first = await _service.OpenAsync(
            () => OpenFileAsync(relativePath, "image/png"),
            relativePath,
            maxWidth: 320);
        Assert.True(first.IsFound);
        Assert.Equal("image/webp", first.ContentType);
        var firstEtag = first.ETag;
        byte[] firstThumb;
        await using (first.Stream)
        {
            using var ms = new MemoryStream();
            await first.Stream!.CopyToAsync(ms);
            firstThumb = ms.ToArray();
        }

        // Replace content at the same relative path.
        await Task.Delay(20);
        await SavePngAsync(relativePath, 800, 800, Color.Orange);
        File.SetLastWriteTimeUtc(Path.Combine(_root, relativePath), DateTime.UtcNow.AddMinutes(1));

        var second = await _service.OpenAsync(
            () => OpenFileAsync(relativePath, "image/png"),
            relativePath,
            maxWidth: 320);
        Assert.True(second.IsFound);
        Assert.Equal("image/webp", second.ContentType);
        Assert.NotEqual(firstEtag, second.ETag);

        byte[] secondThumb;
        await using (second.Stream)
        {
            using var ms = new MemoryStream();
            await second.Stream!.CopyToAsync(ms);
            secondThumb = ms.ToArray();
        }

        Assert.NotEqual(firstThumb, secondThumb);
    }

    private async Task<string> SavePngAsync(string relativePath, int width, int height, Color color)
    {
        var absolute = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        using var image = new Image<Rgba32>(width, height, color);
        await image.SaveAsync(absolute, new PngEncoder());
        return relativePath.Replace('\\', '/');
    }

    private Task<(Stream? Stream, string? ContentType)> OpenFileAsync(string relativePath, string contentType)
    {
        var absolute = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absolute))
            return Task.FromResult<(Stream?, string?)>((null, null));

        Stream stream = new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<(Stream?, string?)>((stream, contentType));
    }

    private sealed class TestHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "LabConnectPortal.Tests";
        public string WebRootPath { get; set; } = contentRoot;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
