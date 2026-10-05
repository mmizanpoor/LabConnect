using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Advertisement;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AdvertisementService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : IAdvertisementService
{
    public async Task<OperationResult<List<AdvertisementDto>>> GetAllAsync()
    {
        var items = await context.Advertisements.AsNoTracking()
            .Include(a => a.CreatedByUser)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return OperationResult<List<AdvertisementDto>>.Success(items.Select(Map).ToList());
    }

    public async Task<OperationResult<AdvertisementDto>> GetByIdAsync(Guid advertisementId)
    {
        var entity = await context.Advertisements.AsNoTracking()
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AdvertisementId == advertisementId);

        if (entity == null)
            return OperationResult<AdvertisementDto>.Failure("تبلیغ یافت نشد");

        return OperationResult<AdvertisementDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementDto>> CreateAsync(Guid userId, SaveAdvertisementCommand command)
    {
        var validation = Validate(command);
        if (validation != null)
            return OperationResult<AdvertisementDto>.Failure(validation);

        var now = DateTime.UtcNow.ToLocalTime();
        var entity = new Advertisement
        {
            AdvertisementId = Guid.NewGuid(),
            CreatedAt = now,
            CreatedByUserId = userId,
        };
        Apply(entity, command);

        context.Advertisements.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<AdvertisementDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await context.Entry(entity).Reference(a => a.CreatedByUser).LoadAsync();
        return OperationResult<AdvertisementDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementDto>> UpdateAsync(UpdateAdvertisementCommand command)
    {
        var entity = await context.Advertisements
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AdvertisementId == command.AdvertisementId);

        if (entity == null)
            return OperationResult<AdvertisementDto>.Failure("تبلیغ یافت نشد");

        var validation = Validate(command);
        if (validation != null)
            return OperationResult<AdvertisementDto>.Failure(validation);

        Apply(entity, command);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<AdvertisementDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<AdvertisementDto>.Success(Map(entity));
    }

    public async Task<OperationResult> DeleteAsync(Guid advertisementId)
    {
        var entity = await context.Advertisements.FirstOrDefaultAsync(a => a.AdvertisementId == advertisementId);
        if (entity == null)
            return OperationResult.Failure("تبلیغ یافت نشد");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        context.Advertisements.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<AdvertisementDto>> UploadImageAsync(Guid advertisementId, IFormFile file)
    {
        var entity = await context.Advertisements
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AdvertisementId == advertisementId);

        if (entity == null)
            return OperationResult<AdvertisementDto>.Failure("تبلیغ یافت نشد");

        var saved = await fileStorageService.SaveAdvertisementImageAsync(advertisementId, file, entity.ImagePath);
        if (!saved.Status || string.IsNullOrWhiteSpace(saved.Data))
            return OperationResult<AdvertisementDto>.Failure(saved.Message ?? "خطا در آپلود تصویر");

        entity.ImagePath = saved.Data;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<AdvertisementDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<AdvertisementDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementDto>> DeleteImageAsync(Guid advertisementId)
    {
        var entity = await context.Advertisements
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AdvertisementId == advertisementId);

        if (entity == null)
            return OperationResult<AdvertisementDto>.Failure("تبلیغ یافت نشد");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        entity.ImagePath = null;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<AdvertisementDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<AdvertisementDto>.Success(Map(entity));
    }

    private static string? Validate(SaveAdvertisementCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان الزامی است";

        if (command.Title.Trim().Length > 300)
            return "عنوان نباید بیشتر از ۳۰۰ کاراکتر باشد";

        if ((command.ShortDescription?.Trim().Length ?? 0) > 500)
            return "توضیح مختصر نباید بیشتر از ۵۰۰ کاراکتر باشد";

        if (command.EndAt < command.StartAt)
            return "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد";

        return null;
    }

    private static void Apply(Advertisement entity, SaveAdvertisementCommand command)
    {
        entity.Title = command.Title.Trim();
        entity.ShortDescription = command.ShortDescription?.Trim() ?? string.Empty;
        entity.StartAt = command.StartAt;
        entity.EndAt = command.EndAt;
        entity.IsActive = command.IsActive;
    }

    private static AdvertisementDto Map(Advertisement entity)
    {
        var user = entity.CreatedByUser;
        var name = user == null
            ? string.Empty
            : $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(name) && user != null)
            name = user.Username;

        return new AdvertisementDto
        {
            AdvertisementId = entity.AdvertisementId,
            Title = entity.Title,
            ShortDescription = entity.ShortDescription,
            ImagePath = entity.ImagePath,
            HasImage = !string.IsNullOrWhiteSpace(entity.ImagePath),
            StartAt = entity.StartAt,
            EndAt = entity.EndAt,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedByUserName = name,
        };
    }

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
