using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.OrganizationLocation;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class OrganizationLocationService(LabConnectDbContext context) : IOrganizationLocationService
{
    public async Task<OperationResult<List<OrganizationLocationDto>>> GetMyLocationsAsync(Guid userId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<List<OrganizationLocationDto>>.Failure(access.Error);

        var items = await GetLocationsForManager(access.User!)
            .AsNoTracking()
            .Include(l => l.Province)
            .OrderBy(l => l.LocationName)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return OperationResult<List<OrganizationLocationDto>>.Success(items);
    }

    public async Task<OperationResult<OrganizationLocationDto>> CreateAsync(Guid userId, CreateOrganizationLocationCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<OrganizationLocationDto>.Failure(access.Error);

        var validation = await ValidateLocationCommandAsync(
            access.User!, command.LocationName, command.ProvinceId, null);
        if (validation != null)
            return OperationResult<OrganizationLocationDto>.Failure(validation);

        var location = new OrganizationLocation
        {
            LocationId = Guid.NewGuid(),
            UserId = access.DataOwnerUserId,
            LocationName = command.LocationName.Trim(),
            ProvinceId = command.ProvinceId,
            Address = command.Address.Trim(),
            PhoneNumber = command.PhoneNumber.Trim(),
        };

        context.OrganizationLocations.Add(location);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<OrganizationLocationDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var created = await context.OrganizationLocations
            .AsNoTracking()
            .Include(l => l.Province)
            .FirstAsync(l => l.LocationId == location.LocationId);

        return OperationResult<OrganizationLocationDto>.Success(MapToDto(created));
    }

    public async Task<OperationResult<OrganizationLocationDto>> UpdateAsync(Guid userId, UpdateOrganizationLocationCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<OrganizationLocationDto>.Failure(access.Error);

        var location = await GetLocationsForManager(access.User!)
            .Include(l => l.Province)
            .FirstOrDefaultAsync(l => l.LocationId == command.LocationId);

        if (location == null)
            return OperationResult<OrganizationLocationDto>.Failure("شعبه یافت نشد");

        var validation = await ValidateLocationCommandAsync(
            access.User!, command.LocationName, command.ProvinceId, command.LocationId);
        if (validation != null)
            return OperationResult<OrganizationLocationDto>.Failure(validation);

        location.LocationName = command.LocationName.Trim();
        location.ProvinceId = command.ProvinceId;
        location.Address = command.Address.Trim();
        location.PhoneNumber = command.PhoneNumber.Trim();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<OrganizationLocationDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<OrganizationLocationDto>.Success(MapToDto(location));
    }

    public async Task<OperationResult> DeleteAsync(Guid userId, Guid locationId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var location = await GetLocationsForManager(access.User!)
            .FirstOrDefaultAsync(l => l.LocationId == locationId);

        if (location == null)
            return OperationResult.Failure("شعبه یافت نشد");

        var usedInActivePosting = await context.JobPostingRequests.AnyAsync(j =>
            j.LocationId == locationId &&
            j.Status == JobPostingStatus.Active);

        if (usedInActivePosting)
            return OperationResult.Failure("این شعبه در آگهی فعال استفاده شده و قابل حذف نیست");

        context.OrganizationLocations.Remove(location);
        return await SaveChangesAsync();
    }

    private async Task<string?> ValidateLocationCommandAsync(
        User user, string locationName, int provinceId, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(locationName))
            return "نام شعبه الزامی است";

        if (!await context.Provinces.AnyAsync(p => p.ProvinceId == provinceId))
            return "استان انتخاب‌شده معتبر نیست";

        var normalized = locationName.Trim();
        var duplicate = await GetLocationsForManager(user).AnyAsync(l =>
            l.LocationName == normalized &&
            (excludeId == null || l.LocationId != excludeId));

        return duplicate ? "شعبه‌ای با این نام قبلاً ثبت شده است" : null;
    }

    private static OrganizationLocationDto MapToDto(OrganizationLocation location)
        => new()
        {
            LocationId = location.LocationId,
            LocationName = location.LocationName,
            ProvinceId = location.ProvinceId,
            ProvinceName = location.Province?.ProvinceName ?? string.Empty,
            Address = location.Address,
            PhoneNumber = location.PhoneNumber,
        };

    private async Task<(User? User, Guid DataOwnerUserId, string? Error)> ValidateManagerAccessAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return (null, Guid.Empty, "کاربر یافت نشد");

        if (user.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
            return (null, Guid.Empty, "این بخش فقط برای مدیر آزمایشگاه و فروشگاه است");

        if (user.UserType == UserType.UserLab && !user.CenterProfileId.HasValue)
            return (null, Guid.Empty, "پروفایل مرکز برای حساب کاربری تعریف نشده است");

        if (user.UserType == UserType.UserLab)
        {
            var adminUserId = await context.Users.AsNoTracking()
                .Where(candidate =>
                    candidate.UserType == UserType.AdminLab &&
                    candidate.CenterProfileId == user.CenterProfileId)
                .Select(candidate => (Guid?)candidate.Id)
                .FirstOrDefaultAsync();

            return (user, adminUserId ?? user.Id, null);
        }

        return (user, user.Id, null);
    }

    private IQueryable<OrganizationLocation> GetLocationsForManager(User user)
    {
        var centerProfileId = user.CenterProfileId;
        return context.OrganizationLocations.Where(location =>
            location.UserId == user.Id ||
            (centerProfileId.HasValue && location.User.CenterProfileId == centerProfileId));
    }

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }
    }
}
