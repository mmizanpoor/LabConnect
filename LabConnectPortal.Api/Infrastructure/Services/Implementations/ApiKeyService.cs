using System.Security.Cryptography;
using System.Text;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ApiKeyService(
    IApiKeyRepository apiKeyRepository,
    IUserRepository userRepository) : IApiKeyService
{
    private const string AccessDeniedMessage = "شما دسترسی ندارید. برای دریافت دسترسی با شرکت تماس حاصل نمایید.";

    public async Task<OperationResult<List<ApiKeyDto>>> GetMineAsync(Guid userId)
    {
        var access = await ResolveCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<List<ApiKeyDto>>.Failure(access.Error);

        var items = await apiKeyRepository.GetByCenterProfileIdAsync(access.CenterProfile!.Id);
        return OperationResult<List<ApiKeyDto>>.Success(
            items.Select(x => Map(x, access.CenterProfile.Name)).ToList());
    }

    public async Task<OperationResult<ApiKeyDto>> CreateAsync(
        Guid userId,
        CreateApiKeyCommand command)
    {
        var access = await ResolveCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<ApiKeyDto>.Failure(access.Error);

        var name = command.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<ApiKeyDto>.Failure("نام کلید الزامی است");
        if (name.Length > 200)
            return OperationResult<ApiKeyDto>.Failure("نام کلید نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد");

        string rawKey;
        string keyHash;
        do
        {
            rawKey = GenerateKey(name);
            keyHash = HashKey(rawKey);
        } while (await apiKeyRepository.ExistsByHashAsync(keyHash));

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            Name = name,
            KeyName = keyHash,
            KeyPrefix = rawKey[..3],
            KeySuffix = rawKey[^3..],
            KeyLength = rawKey.Length,
            CreateDate = DateTime.UtcNow,
            AllowAdd = command.AllowAdd,
            AllowEdit = command.AllowEdit,
            AllowView = command.AllowView,
            CenterProfileId = access.CenterProfile!.Id,
        };

        apiKeyRepository.Add(entity);
        var save = await apiKeyRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ApiKeyDto>.Failure(save.Message ?? "خطا در ایجاد کلید");

        var result = Map(entity, access.CenterProfile.Name);
        result.KeyName = rawKey;
        return OperationResult<ApiKeyDto>.Success(result);
    }

    public async Task<OperationResult<ApiKeyDto>> UpdateMineAsync(
        Guid userId,
        UpdateApiKeyCommand command)
    {
        var access = await ResolveCenterAsync(userId);
        if (access.Error != null)
            return OperationResult<ApiKeyDto>.Failure(access.Error);

        var entity = await apiKeyRepository.GetByIdForCenterAsync(command.Id, access.CenterProfile!.Id);
        if (entity == null)
            return OperationResult<ApiKeyDto>.Failure("کلید یافت نشد");

        var validation = ValidateUpdate(command);
        if (validation != null)
            return OperationResult<ApiKeyDto>.Failure(validation);

        ApplyUpdate(entity, command);
        var save = await apiKeyRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ApiKeyDto>.Failure(save.Message ?? "خطا در ویرایش کلید");

        return OperationResult<ApiKeyDto>.Success(Map(entity, access.CenterProfile.Name));
    }

    public async Task<OperationResult> DeleteMineAsync(Guid userId, Guid id)
    {
        var access = await ResolveCenterAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var entity = await apiKeyRepository.GetByIdForCenterAsync(id, access.CenterProfile!.Id);
        if (entity == null)
            return OperationResult.Failure("کلید یافت نشد");

        apiKeyRepository.Delete(entity);
        return await apiKeyRepository.SaveChangesAsync();
    }

    public async Task<OperationResult<List<ApiKeyDto>>> GetAllAsync()
    {
        var items = await apiKeyRepository.GetAllWithCenterAsync();
        return OperationResult<List<ApiKeyDto>>.Success(
            items.Select(x => Map(x, x.CenterProfile?.Name ?? string.Empty)).ToList());
    }

    public async Task<OperationResult<ApiKeyDto>> UpdateAsync(UpdateApiKeyCommand command)
    {
        var entity = await apiKeyRepository.GetByIdWithCenterAsync(command.Id);
        if (entity == null)
            return OperationResult<ApiKeyDto>.Failure("کلید یافت نشد");

        var validation = ValidateUpdate(command);
        if (validation != null)
            return OperationResult<ApiKeyDto>.Failure(validation);

        ApplyUpdate(entity, command);
        var save = await apiKeyRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ApiKeyDto>.Failure(save.Message ?? "خطا در ویرایش کلید");

        return OperationResult<ApiKeyDto>.Success(
            Map(entity, entity.CenterProfile?.Name ?? string.Empty));
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var entity = await apiKeyRepository.GetByIdWithCenterAsync(id);
        if (entity == null)
            return OperationResult.Failure("کلید یافت نشد");

        apiKeyRepository.Delete(entity);
        return await apiKeyRepository.SaveChangesAsync();
    }

    public async Task<OperationResult<ApiKeyAuthorizationDto>> AuthorizeAsync(
        string? rawKey,
        ApiKeyPermission permission)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            return OperationResult<ApiKeyAuthorizationDto>.Failure(AccessDeniedMessage);

        var entity = await apiKeyRepository.GetByHashWithCenterAsync(HashKey(rawKey.Trim()));
        var center = entity?.CenterProfile;
        if (entity == null || center == null || !center.IsApiKeyEnabled)
            return OperationResult<ApiKeyAuthorizationDto>.Failure(AccessDeniedMessage);

        var allowed = permission switch
        {
            ApiKeyPermission.Add => entity.AllowAdd,
            ApiKeyPermission.Edit => entity.AllowEdit,
            ApiKeyPermission.View => entity.AllowView,
            _ => true,
        };
        if (!allowed)
            return OperationResult<ApiKeyAuthorizationDto>.Failure(AccessDeniedMessage);

        var apiUser = center.OwnerUserId.HasValue
            ? await userRepository.GetWithRolesAsync(center.OwnerUserId.Value)
            : null;
        if (apiUser == null ||
            !apiUser.IsActive ||
            apiUser.CenterProfileId != center.Id ||
            apiUser.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
        {
            apiUser = (await userRepository.GetByCenterProfileIdAsync(center.Id))
                .FirstOrDefault(x =>
                    x.IsActive &&
                    x.UserType is UserType.AdminLab or UserType.UserLab or UserType.Store);
        }
        if (apiUser == null)
            return OperationResult<ApiKeyAuthorizationDto>.Failure(AccessDeniedMessage);

        return OperationResult<ApiKeyAuthorizationDto>.Success(new ApiKeyAuthorizationDto
        {
            ApiKeyId = entity.Id,
            CenterProfileId = entity.CenterProfileId,
            UserId = apiUser.Id,
        });
    }

    private async Task<(CenterProfile? CenterProfile, string? Error)> ResolveCenterAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || user.UserType is not (UserType.AdminLab or UserType.Store))
            return (null, "دسترسی مجاز نیست");
        if (user.CenterProfile == null)
            return (null, "پروفایل مرکز یافت نشد");
        if (!user.CenterProfile.IsApiKeyEnabled)
            return (null, "قابلیت کلید API برای این مرکز فعال نشده است");

        return (user.CenterProfile, null);
    }

    private static string GenerateKey(string name)
    {
        var nameHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..8];
        var random = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        return $"PTN-{nameHash}-{random}";
    }

    private static string HashKey(string rawKey)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

    private static ApiKeyDto Map(ApiKey entity, string centerName)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            KeyName = Mask(entity),
            CreateDate = entity.CreateDate,
            AllowAdd = entity.AllowAdd,
            AllowEdit = entity.AllowEdit,
            AllowView = entity.AllowView,
            CenterProfileId = entity.CenterProfileId,
            CenterName = centerName,
        };

    private static string Mask(ApiKey entity)
    {
        var hiddenLength = Math.Max(0, entity.KeyLength - 6);
        return $"{entity.KeyPrefix}{new string('*', hiddenLength)}{entity.KeySuffix}";
    }

    private static string? ValidateUpdate(UpdateApiKeyCommand command)
    {
        var name = command.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            return "نام کلید الزامی است";
        return name.Length > 200
            ? "نام کلید نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد"
            : null;
    }

    private static void ApplyUpdate(ApiKey entity, UpdateApiKeyCommand command)
    {
        entity.Name = command.Name.Trim();
        entity.AllowAdd = command.AllowAdd;
        entity.AllowEdit = command.AllowEdit;
        entity.AllowView = command.AllowView;
    }
}
