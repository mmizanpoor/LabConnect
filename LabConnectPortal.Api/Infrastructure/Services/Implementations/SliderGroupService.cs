using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SliderGroupService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : ISliderGroupService
{
    public async Task<OperationResult<PagedResult<SliderGroupListItemDto>>> GetAllAsync(GetSliderGroupsQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.SliderGroups.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(g => g.Title.Contains(query.Title.Trim()));
        if (query.IsActive.HasValue)
            dbQuery = dbQuery.Where(g => g.IsActive == query.IsActive.Value);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(g => g.SortOrder)
            .ThenByDescending(g => g.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new SliderGroupListItemDto
            {
                Id = g.Id,
                Title = g.Title,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                IsActive = g.IsActive,
                SortOrder = g.SortOrder,
                SlideCount = g.Slides.Count,
                UpdatedAt = g.UpdatedAt,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<SliderGroupDto>> GetByIdAsync(Guid id)
    {
        var group = await LoadGroupAsync(id);
        if (group == null)
            return OperationResult<SliderGroupDto>.Failure("گروه اسلایدر یافت نشد");

        return OperationResult<SliderGroupDto>.Success(MapGroup(group));
    }

    public async Task<OperationResult<SliderGroupDto>> CreateAsync(SaveSliderGroupCommand command)
    {
        var validation = ValidateGroupDates(command.StartDate, command.EndDate, command.Title);
        if (validation != null)
            return OperationResult<SliderGroupDto>.Failure(validation);

        var group = new SliderGroup
        {
            Id = Guid.NewGuid(),
            Title = command.Title.Trim(),
            StartDate = command.StartDate.Date,
            EndDate = command.EndDate.Date,
            IsActive = command.IsActive,
            SortOrder = command.SortOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        context.SliderGroups.Add(group);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SliderGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var created = await LoadGroupAsync(group.Id);
        return OperationResult<SliderGroupDto>.Success(MapGroup(created!));
    }

    public async Task<OperationResult<SliderGroupDto>> UpdateAsync(UpdateSliderGroupCommand command)
    {
        var group = await context.SliderGroups.FirstOrDefaultAsync(g => g.Id == command.Id);
        if (group == null)
            return OperationResult<SliderGroupDto>.Failure("گروه اسلایدر یافت نشد");

        var validation = ValidateGroupDates(command.StartDate, command.EndDate, command.Title);
        if (validation != null)
            return OperationResult<SliderGroupDto>.Failure(validation);

        group.Title = command.Title.Trim();
        group.StartDate = command.StartDate.Date;
        group.EndDate = command.EndDate.Date;
        group.IsActive = command.IsActive;
        group.SortOrder = command.SortOrder;
        group.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SliderGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var updated = await LoadGroupAsync(group.Id);
        return OperationResult<SliderGroupDto>.Success(MapGroup(updated!));
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var group = await context.SliderGroups
            .Include(g => g.Slides)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group == null)
            return OperationResult.Failure("گروه اسلایدر یافت نشد");

        foreach (var slide in group.Slides)
            fileStorageService.DeleteFileIfExists(slide.ImagePath);

        context.SliderGroups.Remove(group);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<SliderSlideDto>> UploadSlideImageAsync(Guid sliderGroupId, IFormFile file)
    {
        var group = await context.SliderGroups
            .Include(g => g.Slides)
            .FirstOrDefaultAsync(g => g.Id == sliderGroupId);
        if (group == null)
            return OperationResult<SliderSlideDto>.Failure("گروه اسلایدر یافت نشد");

        var saveResult = await fileStorageService.SaveSliderSlideImageAsync(sliderGroupId, file);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<SliderSlideDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        var sortOrder = group.Slides.Count == 0 ? 0 : group.Slides.Max(s => s.SortOrder) + 1;
        var slide = new SliderSlide
        {
            Id = Guid.NewGuid(),
            SliderGroupId = sliderGroupId,
            ImagePath = saveResult.Data,
            Title = Path.GetFileNameWithoutExtension(file.FileName) ?? string.Empty,
            SortOrder = sortOrder,
        };

        context.SliderSlides.Add(slide);
        group.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SliderSlideDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SliderSlideDto>.Success(MapSlide(slide));
    }

    public async Task<OperationResult<SliderSlideDto>> UpdateSlideAsync(UpdateSliderSlideCommand command)
    {
        var slide = await context.SliderSlides.FirstOrDefaultAsync(s =>
            s.Id == command.Id && s.SliderGroupId == command.SliderGroupId);
        if (slide == null)
            return OperationResult<SliderSlideDto>.Failure("اسلاید یافت نشد");

        slide.Title = command.Title?.Trim() ?? string.Empty;
        slide.LinkUrl = string.IsNullOrWhiteSpace(command.LinkUrl) ? null : command.LinkUrl.Trim();
        slide.SortOrder = command.SortOrder;

        var group = await context.SliderGroups.FirstOrDefaultAsync(g => g.Id == command.SliderGroupId);
        if (group != null)
            group.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SliderSlideDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SliderSlideDto>.Success(MapSlide(slide));
    }

    public async Task<OperationResult> DeleteSlideAsync(DeleteSliderSlideCommand command)
    {
        var slide = await context.SliderSlides.FirstOrDefaultAsync(s =>
            s.Id == command.Id && s.SliderGroupId == command.SliderGroupId);
        if (slide == null)
            return OperationResult.Failure("اسلاید یافت نشد");

        fileStorageService.DeleteFileIfExists(slide.ImagePath);
        context.SliderSlides.Remove(slide);

        var group = await context.SliderGroups.FirstOrDefaultAsync(g => g.Id == command.SliderGroupId);
        if (group != null)
            group.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        return await SaveChangesAsync();
    }

    public async Task<OperationResult> ReorderSlidesAsync(ReorderSlidesCommand command)
    {
        var slides = await context.SliderSlides
            .Where(s => s.SliderGroupId == command.SliderGroupId)
            .ToListAsync();

        for (var i = 0; i < command.SlideIds.Count; i++)
        {
            var slide = slides.FirstOrDefault(s => s.Id == command.SlideIds[i]);
            if (slide != null)
                slide.SortOrder = i;
        }

        var group = await context.SliderGroups.FirstOrDefaultAsync(g => g.Id == command.SliderGroupId);
        if (group != null)
            group.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        return await SaveChangesAsync();
    }

    public Task<(Stream? Stream, string? ContentType)> OpenSlideImageAsync(string path)
        => fileStorageService.OpenSiteFileAsync(path);

    public async Task<OperationResult<List<ActiveSliderSlideDto>>> GetActiveSlidesAsync()
    {
        var today = DateTime.Today;
        var groups = await context.SliderGroups.AsNoTracking()
            .Include(g => g.Slides)
            .Where(g => g.IsActive && g.StartDate.Date <= today && g.EndDate.Date >= today)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Title)
            .ToListAsync();

        var slides = groups
            .SelectMany(g => g.Slides.OrderBy(s => s.SortOrder))
            .Select((slide, index) => new ActiveSliderSlideDto
            {
                Id = slide.Id,
                ImagePath = slide.ImagePath,
                LinkUrl = slide.LinkUrl,
                Title = slide.Title,
                SortOrder = index,
            })
            .ToList();

        return OperationResult<List<ActiveSliderSlideDto>>.Success(slides);
    }

    private async Task<SliderGroup?> LoadGroupAsync(Guid id)
        => await context.SliderGroups.AsNoTracking()
            .Include(g => g.Slides.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(g => g.Id == id);

    private static SliderGroupDto MapGroup(SliderGroup group) => new()
    {
        Id = group.Id,
        Title = group.Title,
        StartDate = group.StartDate,
        EndDate = group.EndDate,
        IsActive = group.IsActive,
        SortOrder = group.SortOrder,
        CreatedAt = group.CreatedAt,
        UpdatedAt = group.UpdatedAt,
        Slides = group.Slides.Select(MapSlide).ToList(),
    };

    private static SliderSlideDto MapSlide(SliderSlide slide) => new()
    {
        Id = slide.Id,
        SliderGroupId = slide.SliderGroupId,
        ImagePath = slide.ImagePath,
        LinkUrl = slide.LinkUrl,
        Title = slide.Title,
        SortOrder = slide.SortOrder,
    };

    private static string? ValidateGroupDates(DateTime startDate, DateTime endDate, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "عنوان گروه الزامی است";
        if (startDate.Date > endDate.Date)
            return "تاریخ شروع نمی‌تواند بعد از تاریخ پایان باشد";
        return null;
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

    private static int NormalizePage(int page) => page < 1 ? 1 : page;
    private static int NormalizePageSize(int pageSize) => pageSize < 1 ? 20 : Math.Min(pageSize, 100);

    private static OperationResult<PagedResult<T>> SuccessPaged<T>(List<T> items, int totalCount, int page, int pageSize)
        => OperationResult<PagedResult<T>>.Success(new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
}
