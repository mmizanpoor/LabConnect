using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AdvertisementPositionService(LabConnectDbContext context) : IAdvertisementPositionService
{
    public async Task<OperationResult<List<AdvertisementPositionDto>>> GetAllAsync(bool activeOnly = false)
    {
        var query = context.AdvertisementPositions.AsNoTracking()
            .Where(x => x.Code != "job_posting");
        if (activeOnly)
            query = query.Where(x => x.IsActive);

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => Map(x))
            .ToListAsync();

        return OperationResult<List<AdvertisementPositionDto>>.Success(items);
    }

    public async Task<OperationResult<AdvertisementPositionDto>> GetByIdAsync(int id)
    {
        var entity = await context.AdvertisementPositions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return OperationResult<AdvertisementPositionDto>.Failure("جایگاه یافت نشد");

        return OperationResult<AdvertisementPositionDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementPositionDto>> CreateAsync(SaveAdvertisementPositionCommand command)
    {
        var validation = Validate(command);
        if (validation != null)
            return OperationResult<AdvertisementPositionDto>.Failure(validation);

        var code = command.Code.Trim().ToLowerInvariant();
        if (await context.AdvertisementPositions.AnyAsync(x => x.Code == code))
            return OperationResult<AdvertisementPositionDto>.Failure("کد جایگاه تکراری است");

        var entity = new AdvertisementPosition
        {
            Code = code,
            Title = command.Title.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            MaxConcurrentSlots = command.MaxConcurrentSlots,
            MaxDisplayCount = command.MaxDisplayCount,
            IsActive = command.IsActive,
            CreatedAt = DateTime.UtcNow.ToLocalTime(),
        };

        context.AdvertisementPositions.Add(entity);
        await context.SaveChangesAsync();
        return OperationResult<AdvertisementPositionDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementPositionDto>> UpdateAsync(UpdateAdvertisementPositionCommand command)
    {
        var entity = await context.AdvertisementPositions.FirstOrDefaultAsync(x => x.Id == command.Id);
        if (entity == null)
            return OperationResult<AdvertisementPositionDto>.Failure("جایگاه یافت نشد");

        var validation = Validate(command);
        if (validation != null)
            return OperationResult<AdvertisementPositionDto>.Failure(validation);

        var code = command.Code.Trim().ToLowerInvariant();
        if (await context.AdvertisementPositions.AnyAsync(x => x.Code == code && x.Id != command.Id))
            return OperationResult<AdvertisementPositionDto>.Failure("کد جایگاه تکراری است");

        entity.Code = code;
        entity.Title = command.Title.Trim();
        entity.Description = command.Description?.Trim() ?? string.Empty;
        entity.MaxConcurrentSlots = command.MaxConcurrentSlots;
        entity.MaxDisplayCount = command.MaxDisplayCount;
        entity.IsActive = command.IsActive;

        await context.SaveChangesAsync();
        return OperationResult<AdvertisementPositionDto>.Success(Map(entity));
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var entity = await context.AdvertisementPositions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return OperationResult.Failure("جایگاه یافت نشد");

        var hasOrders = await context.AdvertisementOrders.AnyAsync(x => x.AdvertisementPositionId == id);
        if (hasOrders)
            return OperationResult.Failure("این جایگاه دارای سفارش است و قابل حذف نیست");

        var hasPrices = await context.AdvertisementPrices.AnyAsync(x => x.AdvertisementPositionId == id);
        if (hasPrices)
            return OperationResult.Failure("این جایگاه دارای تعرفه است و قابل حذف نیست");

        context.AdvertisementPositions.Remove(entity);
        await context.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    private static string? Validate(SaveAdvertisementPositionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Code))
            return "کد جایگاه الزامی است";
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان جایگاه الزامی است";
        if (command.MaxConcurrentSlots < 1)
            return "ظرفیت فروش باید حداقل ۱ باشد";
        if (command.MaxDisplayCount is < 1)
            return "حداکثر نمایش باید حداقل ۱ باشد";
        return null;
    }

    private static AdvertisementPositionDto Map(AdvertisementPosition entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Title = entity.Title,
        Description = entity.Description,
        MaxConcurrentSlots = entity.MaxConcurrentSlots,
        MaxDisplayCount = entity.MaxDisplayCount,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
    };
}

public class AdvertisementDurationService(LabConnectDbContext context) : IAdvertisementDurationService
{
    public async Task<OperationResult<List<AdvertisementDurationDto>>> GetAllAsync()
    {
        var items = await context.AdvertisementDurations.AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .Select(x => new AdvertisementDurationDto
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                DaysCount = x.DaysCount,
                SortOrder = x.SortOrder,
            })
            .ToListAsync();

        return OperationResult<List<AdvertisementDurationDto>>.Success(items);
    }
}
