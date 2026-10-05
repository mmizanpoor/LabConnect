using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AdvertisementPriceService(LabConnectDbContext context) : IAdvertisementPriceService
{
    private static DateTime ToLocalDate(DateTime value) => value.ToLocalTime().Date;

    public async Task<OperationResult<AdvertisementPriceMatrixDto>> GetMatrixAsync()
    {
        var positions = await context.AdvertisementPositions.AsNoTracking()
            .Where(x => x.IsActive && x.Code != "job_posting")
            .OrderBy(x => x.Id)
            .Select(x => new AdvertisementPositionDto
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Description = x.Description,
                MaxConcurrentSlots = x.MaxConcurrentSlots,
                MaxDisplayCount = x.MaxDisplayCount,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
            })
            .ToListAsync();

        var durations = await context.AdvertisementDurations.AsNoTracking()
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

        var today = ToLocalDate(DateTime.UtcNow);
        var currentPrices = await context.AdvertisementPrices.AsNoTracking()
            .Where(x => x.ValidFrom.Date <= today && (x.ValidTo == null || x.ValidTo.Value.Date > today))
            .ToListAsync();

        var cells = new List<AdvertisementPriceMatrixCellDto>();
        foreach (var position in positions)
        {
            foreach (var duration in durations)
            {
                var current = currentPrices
                    .Where(x => x.AdvertisementPositionId == position.Id && x.AdvertisementDurationId == duration.Id)
                    .OrderByDescending(x => x.ValidFrom)
                    .FirstOrDefault();

                cells.Add(new AdvertisementPriceMatrixCellDto
                {
                    AdvertisementPositionId = position.Id,
                    AdvertisementDurationId = duration.Id,
                    CurrentPrice = current?.Price,
                    CurrentPriceId = current?.Id,
                    ValidFrom = current?.ValidFrom,
                });
            }
        }

        return OperationResult<AdvertisementPriceMatrixDto>.Success(new AdvertisementPriceMatrixDto
        {
            Positions = positions,
            Durations = durations,
            Cells = cells,
        });
    }

    public async Task<OperationResult<AdvertisementPriceDto?>> GetCurrentPriceAsync(
        int positionId,
        int durationId,
        DateTime? asOf = null)
    {
        var at = ToLocalDate(asOf ?? DateTime.UtcNow);
        var entity = await FindCurrentPriceEntityAsync(positionId, durationId, at);
        if (entity == null)
            return OperationResult<AdvertisementPriceDto?>.Success(null);

        return OperationResult<AdvertisementPriceDto?>.Success(await MapAsync(entity, at));
    }

    public async Task<OperationResult<List<AdvertisementPriceDto>>> GetHistoryAsync(GetAdvertisementPriceHistoryQuery query)
    {
        var items = await context.AdvertisementPrices.AsNoTracking()
            .Include(x => x.Position)
            .Include(x => x.Duration)
            .Include(x => x.CreatedByUser)
            .Where(x =>
                x.AdvertisementPositionId == query.AdvertisementPositionId &&
                x.AdvertisementDurationId == query.AdvertisementDurationId)
            .OrderByDescending(x => x.ValidFrom)
            .ToListAsync();

        var today = ToLocalDate(DateTime.UtcNow);
        var mapped = new List<AdvertisementPriceDto>();
        foreach (var item in items)
            mapped.Add(await MapAsync(item, today));

        return OperationResult<List<AdvertisementPriceDto>>.Success(mapped);
    }

    public async Task<OperationResult<AdvertisementPriceDto>> SetPriceAsync(Guid userId, SetAdvertisementPriceCommand command)
    {
        if (command.Price < 0)
            return OperationResult<AdvertisementPriceDto>.Failure("قیمت نامعتبر است");

        var positionExists = await context.AdvertisementPositions.AnyAsync(x => x.Id == command.AdvertisementPositionId);
        if (!positionExists)
            return OperationResult<AdvertisementPriceDto>.Failure("جایگاه یافت نشد");

        var durationExists = await context.AdvertisementDurations.AnyAsync(x => x.Id == command.AdvertisementDurationId);
        if (!durationExists)
            return OperationResult<AdvertisementPriceDto>.Failure("مدت یافت نشد");

        var validFrom = ToLocalDate(command.ValidFrom ?? DateTime.UtcNow);

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var openPrices = await context.AdvertisementPrices
                .Where(x =>
                    x.AdvertisementPositionId == command.AdvertisementPositionId &&
                    x.AdvertisementDurationId == command.AdvertisementDurationId &&
                    x.ValidTo == null)
                .ToListAsync();

            foreach (var open in openPrices)
            {
                if (open.ValidFrom.Date >= validFrom)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<AdvertisementPriceDto>.Failure("تاریخ اعتبار باید بعد از تعرفه فعلی باشد");
                }

                open.ValidTo = validFrom;
            }

            var entity = new AdvertisementPrice
            {
                Id = Guid.NewGuid(),
                AdvertisementPositionId = command.AdvertisementPositionId,
                AdvertisementDurationId = command.AdvertisementDurationId,
                Price = command.Price,
                ValidFrom = validFrom,
                ValidTo = null,
                CreatedAt = DateTime.UtcNow.ToLocalTime(),
                CreatedByUserId = userId,
            };

            context.AdvertisementPrices.Add(entity);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await context.Entry(entity).Reference(x => x.Position).LoadAsync();
            await context.Entry(entity).Reference(x => x.Duration).LoadAsync();
            await context.Entry(entity).Reference(x => x.CreatedByUser).LoadAsync();

            return OperationResult<AdvertisementPriceDto>.Success(await MapAsync(entity, validFrom));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<AdvertisementPrice?> FindCurrentPriceEntityAsync(int positionId, int durationId, DateTime asOfDate)
    {
        var date = ToLocalDate(asOfDate);
        return await context.AdvertisementPrices.AsNoTracking()
            .Include(x => x.Position)
            .Include(x => x.Duration)
            .Include(x => x.CreatedByUser)
            .Where(x =>
                x.AdvertisementPositionId == positionId &&
                x.AdvertisementDurationId == durationId &&
                x.ValidFrom.Date <= date &&
                (x.ValidTo == null || x.ValidTo.Value.Date > date))
            .OrderByDescending(x => x.ValidFrom)
            .FirstOrDefaultAsync();
    }

    private static Task<AdvertisementPriceDto> MapAsync(AdvertisementPrice entity, DateTime asOfDate)
    {
        var date = ToLocalDate(asOfDate);
        var isCurrent = entity.ValidFrom.Date <= date && (entity.ValidTo == null || entity.ValidTo.Value.Date > date);
        return Task.FromResult(new AdvertisementPriceDto
        {
            Id = entity.Id,
            AdvertisementPositionId = entity.AdvertisementPositionId,
            PositionTitle = entity.Position?.Title ?? string.Empty,
            AdvertisementDurationId = entity.AdvertisementDurationId,
            DurationTitle = entity.Duration?.Title ?? string.Empty,
            Price = entity.Price,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            CreatedAt = entity.CreatedAt,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedByUserName = entity.CreatedByUser == null
                ? string.Empty
                : $"{entity.CreatedByUser.FirstName} {entity.CreatedByUser.LastName}".Trim(),
            IsCurrent = isCurrent,
        });
    }
}
