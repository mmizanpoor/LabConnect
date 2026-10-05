using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabAgreementSettings;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LabAgreementSettingsService(
    LabConnectDbContext context,
    IUserRepository userRepository,
    IFileStorageService fileStorageService) : ILabAgreementSettingsService
{
    public async Task<OperationResult<LabAgreementSettingsDto>> GetMineAsync(Guid userId)
    {
        var access = await ResolveLabCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<LabAgreementSettingsDto>.Failure(access.Error);

        var settings = await context.LabAgreementSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CenterProfileId == access.CenterProfileId);

        return OperationResult<LabAgreementSettingsDto>.Success(
            MapDto(settings, access.CenterProfileId!.Value, access.CenterProfile));
    }

    public async Task<OperationResult<LabAgreementSettingsDto>> GetByLabCodeNewAsync(int labCodeNew)
    {
        var profile = await context.CenterProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.LabCodeNew == labCodeNew &&
                p.CenterType == CenterType.Lab);

        if (profile == null)
            return OperationResult<LabAgreementSettingsDto>.Failure("آزمایشگاه با این کد یافت نشد");

        var settings = await context.LabAgreementSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CenterProfileId == profile.Id);

        return OperationResult<LabAgreementSettingsDto>.Success(MapDto(settings, profile.Id, profile));
    }

    public async Task<OperationResult<LabAgreementSettingsDto>> UpsertAsync(
        Guid userId,
        UpsertLabAgreementSettingsCommand command)
    {
        var access = await ResolveLabCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<LabAgreementSettingsDto>.Failure(access.Error);

        var labName = command.LabName?.Trim() ?? string.Empty;
        var headerAddress = command.HeaderAddress?.Trim() ?? string.Empty;
        var description1 = command.Description1?.Trim() ?? string.Empty;

        if (labName.Length > 200)
            return OperationResult<LabAgreementSettingsDto>.Failure("نام آزمایشگاه بیش از حد طولانی است");
        if (headerAddress.Length > 500)
            return OperationResult<LabAgreementSettingsDto>.Failure("آدرس سربرگ بیش از حد طولانی است");
        if (description1.Length > 2000)
            return OperationResult<LabAgreementSettingsDto>.Failure("توضیح ۱ بیش از حد طولانی است");

        var settings = await context.LabAgreementSettings
            .FirstOrDefaultAsync(s => s.CenterProfileId == access.CenterProfileId);

        if (settings == null)
        {
            settings = new LabAgreementSettings
            {
                Id = Guid.NewGuid(),
                CenterProfileId = access.CenterProfileId!.Value,
            };
            context.LabAgreementSettings.Add(settings);
        }

        settings.UseHeaderImage = command.UseHeaderImage;
        settings.LabName = labName;
        settings.HeaderAddress = headerAddress;
        settings.Description1 = description1;
        settings.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<LabAgreementSettingsDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<LabAgreementSettingsDto>.Success(
            MapDto(settings, settings.CenterProfileId, access.CenterProfile));
    }

    public async Task<OperationResult<LabAgreementSettingsDto>> UploadFileAsync(
        Guid userId,
        LabAgreementSettingsFileKind kind,
        IFormFile file)
    {
        var access = await ResolveLabCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<LabAgreementSettingsDto>.Failure(access.Error);

        var settings = await context.LabAgreementSettings
            .FirstOrDefaultAsync(s => s.CenterProfileId == access.CenterProfileId);

        if (settings == null)
        {
            settings = new LabAgreementSettings
            {
                Id = Guid.NewGuid(),
                CenterProfileId = access.CenterProfileId!.Value,
                LabName = access.CenterProfile?.Name ?? string.Empty,
                HeaderAddress = access.CenterProfile?.Address ?? string.Empty,
            };
            context.LabAgreementSettings.Add(settings);
        }

        var existingPath = kind switch
        {
            LabAgreementSettingsFileKind.HeaderImage => settings.HeaderImagePath,
            LabAgreementSettingsFileKind.HeaderLogo => settings.HeaderLogoPath,
            _ => null,
        };

        var saved = await fileStorageService.SaveLabAgreementSettingsFileAsync(
            access.CenterProfileId!.Value,
            kind,
            file,
            existingPath);

        if (!saved.Status || string.IsNullOrWhiteSpace(saved.Data))
            return OperationResult<LabAgreementSettingsDto>.Failure(saved.Message ?? "خطا در آپلود فایل");

        if (kind == LabAgreementSettingsFileKind.HeaderImage)
            settings.HeaderImagePath = saved.Data;
        else
            settings.HeaderLogoPath = saved.Data;

        settings.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<LabAgreementSettingsDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<LabAgreementSettingsDto>.Success(
            MapDto(settings, settings.CenterProfileId, access.CenterProfile));
    }

    public async Task<(Stream? Stream, string? ContentType, string? Error)> GetFileAsync(
        Guid userId,
        LabAgreementSettingsFileKind kind)
    {
        var access = await ResolveLabCenterAsync(userId);
        if (access.Error != null)
            return (null, null, access.Error);

        var settings = await context.LabAgreementSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CenterProfileId == access.CenterProfileId);

        var path = kind switch
        {
            LabAgreementSettingsFileKind.HeaderImage => settings?.HeaderImagePath,
            LabAgreementSettingsFileKind.HeaderLogo => settings?.HeaderLogoPath,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(path))
            return (null, null, "فایل یافت نشد");

        var (stream, contentType) = await fileStorageService.OpenLabAgreementSettingsFileAsync(path);
        if (stream == null)
            return (null, null, "فایل یافت نشد");

        return (stream, contentType, null);
    }

    private async Task<(Guid? CenterProfileId, CenterProfile? CenterProfile, string? Error)> ResolveLabCenterAsync(
        Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || user.UserType is not (UserType.AdminLab or UserType.UserLab))
            return (null, null, "دسترسی فقط برای آزمایشگاه مجاز است");

        var centerProfileId = user.GetCenterProfileId();
        if (!centerProfileId.HasValue)
            return (null, null, "پروفایل مرکز تعریف نشده است");

        var profile = user.CenterProfile;
        if (profile == null)
        {
            profile = await context.CenterProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == centerProfileId.Value);
        }

        if (profile == null)
            return (null, null, "پروفایل مرکز یافت نشد");

        if (profile.CenterType != CenterType.Lab)
            return (null, null, "دسترسی فقط برای نوع آزمایشگاه مجاز است");

        return (centerProfileId, profile, null);
    }

    private static LabAgreementSettingsDto MapDto(
        LabAgreementSettings? settings,
        Guid centerProfileId,
        CenterProfile? profile)
        => new()
        {
            Id = settings?.Id,
            CenterProfileId = centerProfileId,
            UseHeaderImage = settings?.UseHeaderImage ?? false,
            HeaderImagePath = settings?.HeaderImagePath,
            LabName = settings?.LabName ?? profile?.Name ?? string.Empty,
            HeaderAddress = settings?.HeaderAddress ?? profile?.Address ?? string.Empty,
            Description1 = settings?.Description1 ?? string.Empty,
            HeaderLogoPath = settings?.HeaderLogoPath,
            UpdatedAt = settings?.UpdatedAt,
            HasHeaderImage = !string.IsNullOrWhiteSpace(settings?.HeaderImagePath),
            HasHeaderLogo = !string.IsNullOrWhiteSpace(settings?.HeaderLogoPath),
        };

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure("خطا در ذخیره‌سازی");
        }
    }
}
