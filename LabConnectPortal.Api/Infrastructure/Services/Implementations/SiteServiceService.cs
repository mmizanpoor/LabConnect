using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteService;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteServiceService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : ISiteServiceService, IPublicSiteServiceService
{
    public async Task<OperationResult<List<SiteServiceDto>>> GetAllAsync()
    {
        var items = await context.SiteServices.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Title)
            .ToListAsync();

        return OperationResult<List<SiteServiceDto>>.Success(items.Select(Map).ToList());
    }

    public async Task<OperationResult<SiteServiceDto>> GetByIdAsync(int id)
    {
        var entity = await context.SiteServices.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SiteServiceId == id);

        if (entity == null)
            return OperationResult<SiteServiceDto>.Failure("سرویس یافت نشد");

        return OperationResult<SiteServiceDto>.Success(Map(entity));
    }

    public async Task<OperationResult<SiteServiceDto>> CreateAsync(SaveSiteServiceCommand command)
    {
        var validation = Validate(command);
        if (validation != null)
            return OperationResult<SiteServiceDto>.Failure(validation);

        var entity = new SiteService
        {
            Title = command.Title.Trim(),
            LinkUrl = command.LinkUrl?.Trim(),
            IsActive = command.IsActive,
            SortOrder = command.SortOrder,
        };

        context.SiteServices.Add(entity);

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteServiceDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteServiceDto>.Success(Map(entity));
    }

    public async Task<OperationResult<SiteServiceDto>> UpdateAsync(UpdateSiteServiceCommand command)
    {
        var validation = Validate(command);
        if (validation != null)
            return OperationResult<SiteServiceDto>.Failure(validation);

        var entity = await context.SiteServices.FirstOrDefaultAsync(s => s.SiteServiceId == command.SiteServiceId);
        if (entity == null)
            return OperationResult<SiteServiceDto>.Failure("سرویس یافت نشد");

        entity.Title = command.Title.Trim();
        entity.LinkUrl = command.LinkUrl?.Trim();
        entity.IsActive = command.IsActive;
        entity.SortOrder = command.SortOrder;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteServiceDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteServiceDto>.Success(Map(entity));
    }

    public async Task<OperationResult<SiteServiceDto>> UploadImageAsync(int id, IFormFile file)
    {
        var entity = await context.SiteServices.FirstOrDefaultAsync(s => s.SiteServiceId == id);
        if (entity == null)
            return OperationResult<SiteServiceDto>.Failure("سرویس یافت نشد");

        var saveResult = await fileStorageService.SaveSiteServiceImageAsync(id, file, entity.ImagePath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<SiteServiceDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        entity.ImagePath = saveResult.Data;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteServiceDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteServiceDto>.Success(Map(entity));
    }

    public async Task<OperationResult<SiteServiceDto>> DeleteImageAsync(int id)
    {
        var entity = await context.SiteServices.FirstOrDefaultAsync(s => s.SiteServiceId == id);
        if (entity == null)
            return OperationResult<SiteServiceDto>.Failure("سرویس یافت نشد");

        if (string.IsNullOrWhiteSpace(entity.ImagePath))
            return OperationResult<SiteServiceDto>.Failure("تصویری برای این سرویس ثبت نشده است");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        entity.ImagePath = null;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SiteServiceDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteServiceDto>.Success(Map(entity));
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var entity = await context.SiteServices.FirstOrDefaultAsync(s => s.SiteServiceId == id);
        if (entity == null)
            return OperationResult.Failure("سرویس یافت نشد");

        fileStorageService.DeleteFileIfExists(entity.ImagePath);
        context.SiteServices.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<List<SiteServiceDto>>> GetActiveAsync()
    {
        var items = await context.SiteServices.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Title)
            .ToListAsync();

        return OperationResult<List<SiteServiceDto>>.Success(items.Select(Map).ToList());
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

    private static string? Validate(SaveSiteServiceCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان سرویس الزامی است";

        if (command.SortOrder < 0)
            return "ترتیب نمایش باید مثبت باشد";

        return null;
    }

    private static SiteServiceDto Map(SiteService entity) =>
        new()
        {
            SiteServiceId = entity.SiteServiceId,
            Title = entity.Title,
            ImagePath = entity.ImagePath,
            LinkUrl = entity.LinkUrl,
            IsActive = entity.IsActive,
            SortOrder = entity.SortOrder,
        };
}

