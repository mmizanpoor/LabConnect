using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.KitGroup;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class KitGroupService(LabConnectDbContext context, IUserRepository userRepository) : IKitGroupService
{
    public async Task<OperationResult<List<KitGroupDto>>> GetAllAsync(Guid userId)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<List<KitGroupDto>>.Failure(access.Error);

        var items = await context.KitGroups
            .AsNoTracking()
            .Where(g => g.LabCodeNew == access.LabCodeNew)
            .OrderBy(g => g.Title)
            .Select(g => new KitGroupDto { Id = g.Id, Title = g.Title })
            .ToListAsync();

        return OperationResult<List<KitGroupDto>>.Success(items);
    }

    public async Task<OperationResult<KitGroupDto>> CreateAsync(Guid userId, CreateKitGroupCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<KitGroupDto>.Failure(access.Error);

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<KitGroupDto>.Failure("عنوان الزامی است");

        if (await context.KitGroups.AnyAsync(g => g.LabCodeNew == access.LabCodeNew && g.Title == title))
            return OperationResult<KitGroupDto>.Failure("گروه با این عنوان قبلاً ثبت شده است");

        var entity = new KitGroup { LabCodeNew = access.LabCodeNew!.Value, Title = title };
        context.KitGroups.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<KitGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<KitGroupDto>.Success(new KitGroupDto { Id = entity.Id, Title = entity.Title });
    }

    public async Task<OperationResult<KitGroupDto>> UpdateAsync(Guid userId, UpdateKitGroupCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<KitGroupDto>.Failure(access.Error);

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<KitGroupDto>.Failure("عنوان الزامی است");

        var entity = await context.KitGroups
            .FirstOrDefaultAsync(g => g.Id == command.Id && g.LabCodeNew == access.LabCodeNew);
        if (entity == null)
            return OperationResult<KitGroupDto>.Failure("گروه یافت نشد");

        if (await context.KitGroups.AnyAsync(g =>
                g.LabCodeNew == access.LabCodeNew && g.Title == title && g.Id != command.Id))
            return OperationResult<KitGroupDto>.Failure("گروه با این عنوان قبلاً ثبت شده است");

        entity.Title = title;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<KitGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<KitGroupDto>.Success(new KitGroupDto { Id = entity.Id, Title = entity.Title });
    }

    public async Task<OperationResult> DeleteAsync(Guid userId, long id)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var entity = await context.KitGroups
            .FirstOrDefaultAsync(g => g.Id == id && g.LabCodeNew == access.LabCodeNew);
        if (entity == null)
            return OperationResult.Failure("گروه یافت نشد");

        var inUse = await context.TestInfos.AnyAsync(t => t.KitGroupId == id);
        if (inUse)
            return OperationResult.Failure("این گروه در آزمایشات استفاده شده و قابل حذف نیست");

        context.KitGroups.Remove(entity);
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
