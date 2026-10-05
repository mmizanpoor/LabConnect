using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AdvertisementOrderService(
    LabConnectDbContext context,
    IAdvertisementPriceService priceService) : IAdvertisementOrderService
{
    public async Task<OperationResult<List<AdvertisementOrderDto>>> GetAllAsync()
    {
        var items = await context.AdvertisementOrders.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Position)
            .Include(x => x.Duration)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return OperationResult<List<AdvertisementOrderDto>>.Success(items.Select(Map).ToList());
    }

    public async Task<OperationResult<AdvertisementOrderDto>> GetByIdAsync(Guid id)
    {
        var entity = await context.AdvertisementOrders.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Position)
            .Include(x => x.Duration)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return OperationResult<AdvertisementOrderDto>.Failure("سفارش یافت نشد");

        return OperationResult<AdvertisementOrderDto>.Success(Map(entity));
    }

    public async Task<OperationResult<AdvertisementOrderDto>> CreateAsync(SaveAdvertisementOrderCommand command)
    {
        var userExists = await context.Users.AnyAsync(x => x.Id == command.UserId);
        if (!userExists)
            return OperationResult<AdvertisementOrderDto>.Failure("کاربر یافت نشد");

        var position = await context.AdvertisementPositions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.AdvertisementPositionId);
        if (position == null || !position.IsActive)
            return OperationResult<AdvertisementOrderDto>.Failure("جایگاه فعال یافت نشد");

        var duration = await context.AdvertisementDurations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.AdvertisementDurationId);
        if (duration == null)
            return OperationResult<AdvertisementOrderDto>.Failure("مدت یافت نشد");

        var startDate = command.StartDate.ToLocalTime().Date;

        var priceResult = await priceService.GetCurrentPriceAsync(
            command.AdvertisementPositionId,
            command.AdvertisementDurationId,
            startDate);
        if (!priceResult.Status)
            return OperationResult<AdvertisementOrderDto>.Failure(priceResult.Message ?? "خطا در دریافت تعرفه");
        if (priceResult.Data == null)
            return OperationResult<AdvertisementOrderDto>.Failure("تعرفه فعالی برای این ترکیب ثبت نشده است");

        var endDate = startDate.AddDays(duration.DaysCount);

        if (AdvertisementOrderStatusHelper.OccupiesSlot(command.Status))
        {
            var slotError = await ValidateSlotAvailabilityAsync(
                command.AdvertisementPositionId,
                position.MaxConcurrentSlots,
                startDate,
                endDate,
                null);
            if (slotError != null)
                return OperationResult<AdvertisementOrderDto>.Failure(slotError);
        }

        var entity = new AdvertisementOrder
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            AdvertisementPositionId = command.AdvertisementPositionId,
            AdvertisementDurationId = command.AdvertisementDurationId,
            AdvertisementPriceId = priceResult.Data.Id,
            Price = priceResult.Data.Price,
            StartDate = startDate,
            EndDate = endDate,
            Status = command.Status,
            CreatedAt = DateTime.UtcNow.ToLocalTime(),
        };

        context.AdvertisementOrders.Add(entity);
        await context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<OperationResult<AdvertisementOrderDto>> UpdateStatusAsync(UpdateAdvertisementOrderStatusCommand command)
    {
        var entity = await context.AdvertisementOrders
            .Include(x => x.Position)
            .Include(x => x.Duration)
            .FirstOrDefaultAsync(x => x.Id == command.Id);

        if (entity == null)
            return OperationResult<AdvertisementOrderDto>.Failure("سفارش یافت نشد");

        if (command.StartDate.HasValue)
        {
            entity.StartDate = command.StartDate.Value.ToLocalTime().Date;
            var duration = entity.Duration ?? await context.AdvertisementDurations.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == entity.AdvertisementDurationId);
            if (duration != null)
                entity.EndDate = entity.StartDate.AddDays(duration.DaysCount);
        }

        if (AdvertisementOrderStatusHelper.OccupiesSlot(command.Status))
        {
            var slotError = await ValidateSlotAvailabilityAsync(
                entity.AdvertisementPositionId,
                entity.Position?.MaxConcurrentSlots ?? 1,
                entity.StartDate,
                entity.EndDate,
                entity.Id);
            if (slotError != null)
                return OperationResult<AdvertisementOrderDto>.Failure(slotError);
        }

        entity.Status = command.Status;
        await context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var entity = await context.AdvertisementOrders.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            return OperationResult.Failure("سفارش یافت نشد");

        if (entity.Status is AdvertisementOrderStatus.Active or AdvertisementOrderStatus.Paid)
            return OperationResult.Failure("سفارش فعال یا پرداخت‌شده قابل حذف نیست");

        var linkedAd = await context.Advertisements.FirstOrDefaultAsync(x => x.AdvertisementOrderId == id);
        if (linkedAd != null)
            linkedAd.AdvertisementOrderId = null;

        context.AdvertisementOrders.Remove(entity);
        await context.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    private async Task<string?> ValidateSlotAvailabilityAsync(
        int positionId,
        int maxSlots,
        DateTime startDate,
        DateTime endDate,
        Guid? excludeOrderId)
    {
        var occupyingStatuses = new[]
        {
            AdvertisementOrderStatus.PendingPayment,
            AdvertisementOrderStatus.Paid,
            AdvertisementOrderStatus.Active,
        };

        var overlappingCount = await context.AdvertisementOrders.AsNoTracking()
            .Where(x =>
                x.AdvertisementPositionId == positionId &&
                occupyingStatuses.Contains(x.Status) &&
                x.StartDate < endDate &&
                x.EndDate > startDate &&
                (excludeOrderId == null || x.Id != excludeOrderId))
            .CountAsync();

        if (overlappingCount >= maxSlots)
            return "ظرفیت این جایگاه در بازه انتخاب‌شده تکمیل است";

        return null;
    }

    private static AdvertisementOrderDto Map(AdvertisementOrder entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        UserDisplayName = entity.User == null
            ? string.Empty
            : $"{entity.User.FirstName} {entity.User.LastName}".Trim(),
        AdvertisementPositionId = entity.AdvertisementPositionId,
        PositionTitle = entity.Position?.Title ?? string.Empty,
        AdvertisementDurationId = entity.AdvertisementDurationId,
        DurationTitle = entity.Duration?.Title ?? string.Empty,
        AdvertisementPriceId = entity.AdvertisementPriceId,
        Price = entity.Price,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Status = entity.Status,
        StatusTitle = AdvertisementOrderStatusHelper.ToPersianTitle(entity.Status),
        CreatedAt = entity.CreatedAt,
    };
}
