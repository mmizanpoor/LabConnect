using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteSettingsService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : ISiteSettingsService
{
    private const int SettingsId = 1;

    public async Task<OperationResult<SiteSettingsDto>> GetAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        var links = await LoadUsefulLinksAsync();
        return OperationResult<SiteSettingsDto>.Success(MapToDto(settings, links));
    }

    public async Task<OperationResult<SiteSettingsPublicDto>> GetPublicAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        var links = await LoadUsefulLinksAsync();
        return OperationResult<SiteSettingsPublicDto>.Success(MapToPublicDto(settings, links));
    }

    public async Task<OperationResult<SiteSettingsDto>> UpdateAsync(UpdateSiteSettingsCommand command)
    {
        var validation = ValidateUsefulLinks(command.UsefulLinks);
        if (validation != null)
            return OperationResult<SiteSettingsDto>.Failure(validation);

        var settings = await GetOrCreateSettingsAsync();
        settings.SiteTitle = command.SiteTitle?.Trim() ?? string.Empty;
        settings.Tagline = command.Tagline?.Trim() ?? string.Empty;
        settings.SupportLandline = command.SupportLandline?.Trim() ?? string.Empty;
        settings.SupportMobile = command.SupportMobile?.Trim() ?? string.Empty;
        settings.ENamadEmbedCode = string.IsNullOrWhiteSpace(command.ENamadEmbedCode) ? null : command.ENamadEmbedCode.Trim();
        settings.ENamadLinkUrl = string.IsNullOrWhiteSpace(command.ENamadLinkUrl) ? null : command.ENamadLinkUrl.Trim();
        settings.GoogleMapEmbedCode = string.IsNullOrWhiteSpace(command.GoogleMapEmbedCode) ? null : command.GoogleMapEmbedCode.Trim();
        settings.MetaTitle = command.MetaTitle?.Trim() ?? string.Empty;
        settings.MetaDescription = command.MetaDescription?.Trim() ?? string.Empty;
        settings.MetaKeywords = command.MetaKeywords?.Trim() ?? string.Empty;
        settings.MetaViewport = string.IsNullOrWhiteSpace(command.MetaViewport) ? null : command.MetaViewport.Trim();
        settings.MetaCanonical = string.IsNullOrWhiteSpace(command.MetaCanonical) ? null : command.MetaCanonical.Trim();
        settings.FooterAboutText = command.FooterAboutText?.Trim() ?? string.Empty;
        settings.FooterAddress = command.FooterAddress?.Trim() ?? string.Empty;
        settings.FooterEmail = command.FooterEmail?.Trim() ?? string.Empty;
        settings.FooterCopyrightText = command.FooterCopyrightText?.Trim() ?? string.Empty;
        settings.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        await ReplaceUsefulLinksAsync(command.UsefulLinks);

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteSettingsDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var links = await LoadUsefulLinksAsync();
        return OperationResult<SiteSettingsDto>.Success(MapToDto(settings, links));
    }

    public async Task<OperationResult<SiteSettingsDto>> UploadLogoAsync(IFormFile file)
    {
        var settings = await GetOrCreateSettingsAsync();
        var saveResult = await fileStorageService.SaveSiteLogoAsync(file, settings.LogoPath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<SiteSettingsDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        settings.LogoPath = saveResult.Data;
        settings.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteSettingsDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var links = await LoadUsefulLinksAsync();
        return OperationResult<SiteSettingsDto>.Success(MapToDto(settings, links));
    }

    public async Task<(Stream? Stream, string? ContentType)> OpenLogoAsync()
    {
        var settings = await context.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == SettingsId);
        if (settings?.LogoPath == null)
            return (null, null);

        return await fileStorageService.OpenSiteFileAsync(settings.LogoPath);
    }

    public async Task<OperationResult<SiteSettingsDto>> UploadFooterLogoAsync(IFormFile file)
    {
        var settings = await GetOrCreateSettingsAsync();
        var saveResult = await fileStorageService.SaveSiteFooterLogoAsync(file, settings.FooterLogoPath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<SiteSettingsDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        settings.FooterLogoPath = saveResult.Data;
        settings.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteSettingsDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var links = await LoadUsefulLinksAsync();
        return OperationResult<SiteSettingsDto>.Success(MapToDto(settings, links));
    }

    public async Task<(Stream? Stream, string? ContentType)> OpenFooterLogoAsync()
    {
        var settings = await context.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == SettingsId);
        if (settings?.FooterLogoPath == null)
            return (null, null);

        return await fileStorageService.OpenSiteFileAsync(settings.FooterLogoPath);
    }

    private async Task<List<SiteUsefulLink>> LoadUsefulLinksAsync()
        => await context.SiteUsefulLinks.AsNoTracking()
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Title)
            .ToListAsync();

    private async Task ReplaceUsefulLinksAsync(List<SiteUsefulLinkCommand> commands)
    {
        var existing = await context.SiteUsefulLinks.ToListAsync();
        context.SiteUsefulLinks.RemoveRange(existing);

        for (var i = 0; i < commands.Count; i++)
        {
            var cmd = commands[i];
            context.SiteUsefulLinks.Add(new SiteUsefulLink
            {
                Id = cmd.Id.HasValue && cmd.Id.Value != Guid.Empty ? cmd.Id.Value : Guid.NewGuid(),
                Title = cmd.Title.Trim(),
                Url = cmd.Url.Trim(),
                SortOrder = i,
            });
        }
    }

    private static string? ValidateUsefulLinks(List<SiteUsefulLinkCommand> links)
    {
        foreach (var link in links)
        {
            if (string.IsNullOrWhiteSpace(link.Title))
                return "عنوان لینک مفید الزامی است";

            if (string.IsNullOrWhiteSpace(link.Url))
                return "آدرس لینک مفید الزامی است";

            var url = link.Url.Trim();
            if (!url.StartsWith('/') && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return "آدرس لینک باید با / یا http:// یا https:// شروع شود";
        }

        return null;
    }

    private async Task<SiteSettings> GetOrCreateSettingsAsync()
    {
        var settings = await context.SiteSettings.FirstOrDefaultAsync(s => s.Id == SettingsId);
        if (settings != null)
            return settings;

        settings = new SiteSettings
        {
            Id = SettingsId,
            SiteTitle = "LabConnect Portal",
            UpdatedAt = DateTime.UtcNow,
        };
        context.SiteSettings.Add(settings);
        await context.SaveChangesAsync();
        return settings;
    }

    private static SiteSettingsDto MapToDto(SiteSettings settings, List<SiteUsefulLink> links) => new()
    {
        Id = settings.Id,
        SiteTitle = settings.SiteTitle,
        Tagline = settings.Tagline,
        HasLogo = !string.IsNullOrWhiteSpace(settings.LogoPath),
        HasFooterLogo = !string.IsNullOrWhiteSpace(settings.FooterLogoPath),
        SupportLandline = settings.SupportLandline,
        SupportMobile = settings.SupportMobile,
        ENamadEmbedCode = settings.ENamadEmbedCode,
        ENamadLinkUrl = settings.ENamadLinkUrl,
        GoogleMapEmbedCode = settings.GoogleMapEmbedCode,
        MetaTitle = settings.MetaTitle,
        MetaDescription = settings.MetaDescription,
        MetaKeywords = settings.MetaKeywords,
        MetaViewport = settings.MetaViewport,
        MetaCanonical = settings.MetaCanonical,
        FooterAboutText = settings.FooterAboutText ?? string.Empty,
        FooterAddress = settings.FooterAddress,
        FooterEmail = settings.FooterEmail,
        FooterCopyrightText = settings.FooterCopyrightText,
        UsefulLinks = links.Select(MapLink).ToList(),
        UpdatedAt = settings.UpdatedAt,
    };

    private static SiteSettingsPublicDto MapToPublicDto(SiteSettings settings, List<SiteUsefulLink> links) => new()
    {
        SiteTitle = settings.SiteTitle,
        Tagline = settings.Tagline,
        HasLogo = !string.IsNullOrWhiteSpace(settings.LogoPath),
        HasFooterLogo = !string.IsNullOrWhiteSpace(settings.FooterLogoPath),
        SupportLandline = settings.SupportLandline,
        SupportMobile = settings.SupportMobile,
        ENamadEmbedCode = settings.ENamadEmbedCode,
        ENamadLinkUrl = settings.ENamadLinkUrl,
        GoogleMapEmbedCode = settings.GoogleMapEmbedCode,
        MetaTitle = settings.MetaTitle,
        MetaDescription = settings.MetaDescription,
        MetaKeywords = settings.MetaKeywords,
        MetaViewport = settings.MetaViewport,
        MetaCanonical = settings.MetaCanonical,
        FooterAboutText = settings.FooterAboutText ?? string.Empty,
        FooterAddress = settings.FooterAddress,
        FooterEmail = settings.FooterEmail,
        FooterCopyrightText = settings.FooterCopyrightText,
        UsefulLinks = links.Select(MapLink).ToList(),
    };

    private static SiteUsefulLinkDto MapLink(SiteUsefulLink link) => new()
    {
        Id = link.Id,
        Title = link.Title,
        Url = link.Url,
        SortOrder = link.SortOrder,
    };

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException ex)
        {
            return OperationResult.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
