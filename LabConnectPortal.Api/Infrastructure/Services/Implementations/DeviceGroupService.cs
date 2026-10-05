using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.DeviceGroup;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class DeviceGroupService(LabConnectDbContext context, IUserRepository userRepository) : IDeviceGroupService
{
    public async Task<OperationResult<List<DeviceGroupDto>>> GetAllAsync(Guid userId)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<List<DeviceGroupDto>>.Failure(access.Error);

        var items = await context.DeviceGroups
            .AsNoTracking()
            .Where(g => g.LabCodeNew == access.LabCodeNew)
            .OrderBy(g => g.Title)
            .Select(g => new DeviceGroupDto { Id = g.Id, Title = g.Title })
            .ToListAsync();

        return OperationResult<List<DeviceGroupDto>>.Success(items);
    }

    public async Task<OperationResult<DeviceGroupDto>> CreateAsync(Guid userId, CreateDeviceGroupCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<DeviceGroupDto>.Failure(access.Error);

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<DeviceGroupDto>.Failure("عنوان الزامی است");

        if (await context.DeviceGroups.AnyAsync(g => g.LabCodeNew == access.LabCodeNew && g.Title == title))
            return OperationResult<DeviceGroupDto>.Failure("گروه با این عنوان قبلاً ثبت شده است");

        var entity = new DeviceGroup { LabCodeNew = access.LabCodeNew!.Value, Title = title };
        context.DeviceGroups.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<DeviceGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<DeviceGroupDto>.Success(new DeviceGroupDto { Id = entity.Id, Title = entity.Title });
    }

    public async Task<OperationResult<DeviceGroupDto>> UpdateAsync(Guid userId, UpdateDeviceGroupCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<DeviceGroupDto>.Failure(access.Error);

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<DeviceGroupDto>.Failure("عنوان الزامی است");

        var entity = await context.DeviceGroups
            .FirstOrDefaultAsync(g => g.Id == command.Id && g.LabCodeNew == access.LabCodeNew);
        if (entity == null)
            return OperationResult<DeviceGroupDto>.Failure("گروه یافت نشد");

        if (await context.DeviceGroups.AnyAsync(g =>
                g.LabCodeNew == access.LabCodeNew && g.Title == title && g.Id != command.Id))
            return OperationResult<DeviceGroupDto>.Failure("گروه با این عنوان قبلاً ثبت شده است");

        entity.Title = title;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<DeviceGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<DeviceGroupDto>.Success(new DeviceGroupDto { Id = entity.Id, Title = entity.Title });
    }

    public async Task<OperationResult> DeleteAsync(Guid userId, long id)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var entity = await context.DeviceGroups
            .FirstOrDefaultAsync(g => g.Id == id && g.LabCodeNew == access.LabCodeNew);
        if (entity == null)
            return OperationResult.Failure("گروه یافت نشد");

        var inUse = await context.TestInfos.AnyAsync(t => t.DeviceGroupId == id);
        if (inUse)
            return OperationResult.Failure("این گروه در آزمایشات استفاده شده و قابل حذف نیست");

        context.DeviceGroups.Remove(entity);
        return await SaveChangesAsync();
    }

    private async Task<(int? LabCodeNew, string? Error)> GetLabCodeNewAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || user.UserType is not (UserType.AdminLab or UserType.UserLab))
            return (null, "دسترسی مجاز نیست");

        if (!user.GetLabCodeNew().HasValue)
            return (null, "کد آزمایشگاه تعریف نشده است");

        return (user.GetLabCodeNew()!.Value, null);
    }

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
