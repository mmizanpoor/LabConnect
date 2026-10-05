using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.Storage;

public class LocalFileStorageService(
    IWebHostEnvironment environment,
    IOptions<FileStorageSettings> options) : IFileStorageService
{
    private readonly FileStorageSettings _settings = options.Value;

    public async Task<OperationResult<string>> SaveCenterProfileFileAsync(
        Guid profileId,
        CenterProfileFileKind kind,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = kind switch
        {
            CenterProfileFileKind.Logo => $"logo.{extension}",
            CenterProfileFileKind.NationalCard => $"national-card.{extension}",
            CenterProfileFileKind.License => $"license.{extension}",
            CenterProfileFileKind.OfficialImage => $"official-image.{extension}",
            CenterProfileFileKind.TradeCard => $"trade-card.{extension}",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        var relativePath = BuildRelativePath(_settings.RootPath, profileId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        var directory = Path.GetDirectoryName(absolutePath)!;
        Directory.CreateDirectory(directory);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public async Task<OperationResult<string>> SaveUserProfileFileAsync(
        Guid userId,
        UserProfileFileKind kind,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        var maxSize = kind == UserProfileFileKind.Photo ? 2 * 1024 * 1024 : 8 * 1024 * 1024;
        if (file.Length > maxSize)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = kind switch
        {
            UserProfileFileKind.Photo => new[] { "jpg", "jpeg", "png", "webp" },
            UserProfileFileKind.Resume => new[] { "pdf", "doc", "docx" },
            _ => Array.Empty<string>(),
        };

        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = kind switch
        {
            UserProfileFileKind.Photo => $"photo.{extension}",
            UserProfileFileKind.Resume => $"resume.{extension}",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        var relativePath = BuildRelativePath("user-profiles", userId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        var directory = Path.GetDirectoryName(absolutePath)!;
        Directory.CreateDirectory(directory);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public async Task<OperationResult<string>> SaveProductImageAsync(
        Guid productId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("products", productId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        var directory = Path.GetDirectoryName(absolutePath)!;
        Directory.CreateDirectory(directory);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenProductImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveBrandImageAsync(
        int brandId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp", "svg" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("brands", brandId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenBrandImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveSiteServiceImageAsync(
        int siteServiceId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp", "svg" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("site-services", siteServiceId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenSiteServiceImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveCategoryGroupHomePageImageAsync(
        int productCategoryGroupId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp", "svg" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("category-groups", productCategoryGroupId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenCategoryGroupHomePageImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SavePostFeaturedImageAsync(
        Guid contentPostId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp", "svg" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("posts", contentPostId.ToString("N"), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenPostFeaturedImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveAdvertisementImageAsync(
        Guid advertisementId,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp", "svg" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("advertisements", advertisementId.ToString("N"), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenAdvertisementImageAsync(string relativePath)
        => OpenSiteFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveSiteLogoAsync(IFormFile file, string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = ResolveUploadExtension(file, "jpg", "jpeg", "png", "webp", "svg");
        if (extension == null)
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"logo.{extension}";
        var relativePath = BuildRelativePath("site", fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public async Task<OperationResult<string>> SaveSiteFooterLogoAsync(IFormFile file, string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = ResolveUploadExtension(file, "jpg", "jpeg", "png", "webp", "svg");
        if (extension == null)
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"footer-logo.{extension}";
        var relativePath = BuildRelativePath("site", fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public async Task<OperationResult<string>> SaveSliderSlideImageAsync(Guid sliderGroupId, IFormFile file)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var allowed = new[] { "jpg", "jpeg", "png", "webp" };
        if (!allowed.Contains(extension))
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = BuildRelativePath("sliders", sliderGroupId.ToString(), fileName);

        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenSiteFileAsync(string relativePath)
        => OpenCenterProfileFileAsync(relativePath);

    public Task<(Stream? Stream, string? ContentType)> OpenUserProfileFileAsync(string relativePath)
        => OpenCenterProfileFileAsync(relativePath);

    public async Task<OperationResult<string>> SaveLabAgreementSettingsFileAsync(
        Guid centerProfileId,
        LabAgreementSettingsFileKind kind,
        IFormFile file,
        string? existingRelativePath)
    {
        if (file.Length == 0)
            return OperationResult<string>.Failure("فایل خالی است");

        if (file.Length > _settings.MaxFileSizeBytes)
            return OperationResult<string>.Failure("حجم فایل بیش از حد مجاز است");

        var extension = ResolveUploadExtension(file, "jpg", "jpeg", "png", "webp");
        if (extension == null)
            return OperationResult<string>.Failure("فرمت فایل مجاز نیست");

        var fileName = kind switch
        {
            LabAgreementSettingsFileKind.HeaderImage => $"header-image.{extension}",
            LabAgreementSettingsFileKind.HeaderLogo => $"header-logo.{extension}",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        var relativePath = BuildRelativePath("lab-agreement-settings", centerProfileId.ToString(), fileName);
        var absolutePath = GetAbsolutePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        DeleteFileIfExists(existingRelativePath);

        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return OperationResult<string>.Success(relativePath);
    }

    public Task<(Stream? Stream, string? ContentType)> OpenLabAgreementSettingsFileAsync(string relativePath)
        => OpenCenterProfileFileAsync(relativePath);

    public Task<(Stream? Stream, string? ContentType)> OpenCenterProfileFileAsync(string relativePath)
    {
        if (!TryGetSafeAbsolutePath(relativePath, out var absolutePath) || !File.Exists(absolutePath))
            return Task.FromResult<(Stream?, string?)>((null, null));

        var extension = Path.GetExtension(absolutePath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream",
        };

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<(Stream?, string?)>((stream, contentType));
    }

    public void DeleteFileIfExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        if (!TryGetSafeAbsolutePath(relativePath, out var absolutePath))
            return;

        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }

    private string BuildRelativePath(params string[] segments)
        => Path.Combine([_settings.StorageRoot, .. segments]).Replace('\\', '/');

    private string GetAbsolutePath(string relativePath)
        => Path.Combine(environment.ContentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private bool TryGetSafeAbsolutePath(string relativePath, out string absolutePath)
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

    private static string? ResolveUploadExtension(IFormFile file, params string[] allowedExtensions)
    {
        var allowed = allowedExtensions
            .Select(ext => ext.Trim().TrimStart('.').ToLowerInvariant())
            .Where(ext => !string.IsNullOrEmpty(ext))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension) && allowed.Contains(extension))
            return extension;

        var contentType = file.ContentType?.Split(';', 2)[0].Trim().ToLowerInvariant();
        extension = contentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            "image/svg+xml" => "svg",
            _ => null,
        };

        return extension != null && allowed.Contains(extension) ? extension : null;
    }
}
