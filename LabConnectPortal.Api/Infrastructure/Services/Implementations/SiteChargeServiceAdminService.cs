using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteChargeServiceAdminService(LabConnectDbContext context) : ISiteChargeServiceAdminService
{
    public async Task<OperationResult<List<SiteChargeServiceAdminDto>>> GetAllAsync()
    {
        var items = await context.SiteChargeServices
            .AsNoTracking()
            .Include(s => s.Prices)
            .OrderBy(s => s.Code)
            .ThenBy(s => s.Title)
            .ToListAsync();

        return OperationResult<List<SiteChargeServiceAdminDto>>.Success(items.Select(Map).ToList());
    }

    public async Task<OperationResult<SiteChargeServiceAdminDto>> SaveAsync(
        Guid userId,
        SaveSiteChargeServiceCommand command)
    {
        var validation = Validate(command);
        if (validation != null)
            return OperationResult<SiteChargeServiceAdminDto>.Failure(validation);

        var now = DateTime.UtcNow.ToLocalTime();
        SiteChargeService entity;

        if (command.SiteChargeServiceId is > 0)
        {
            var existing = await context.SiteChargeServices
                .Include(s => s.Prices)
                .FirstOrDefaultAsync(s => s.SiteChargeServiceId == command.SiteChargeServiceId.Value);

            if (existing == null)
                return OperationResult<SiteChargeServiceAdminDto>.Failure("سرویس یافت نشد");

            if (existing.Code != command.Code)
            {
                var codeTaken = await context.SiteChargeServices
                    .AnyAsync(s => s.Code == command.Code && s.SiteChargeServiceId != existing.SiteChargeServiceId);
                if (codeTaken)
                    return OperationResult<SiteChargeServiceAdminDto>.Failure("این کد سرویس قبلاً ثبت شده است");
            }

            entity = existing;
            if (entity.Prices.Count > 0)
                context.SiteChargeServicePrices.RemoveRange(entity.Prices);
            entity.Prices.Clear();
        }
        else
        {
            var exists = await context.SiteChargeServices.AnyAsync(s => s.Code == command.Code);
            if (exists)
                return OperationResult<SiteChargeServiceAdminDto>.Failure("این کد سرویس قبلاً ثبت شده است");

            entity = new SiteChargeService();
            context.SiteChargeServices.Add(entity);
        }

        entity.Code = command.Code;
        entity.Title = command.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        entity.PricingMode = command.PricingMode;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = now;
        entity.UpdatedByUserId = userId;

        for (var i = 0; i < command.Prices.Count; i++)
        {
            var item = command.Prices[i];
            entity.Prices.Add(new SiteChargeServicePrice
            {
                Title = string.IsNullOrWhiteSpace(item.Title) ? null : item.Title.Trim(),
                MinQuantity = entity.PricingMode == SiteChargePricingMode.Range ? (item.MinQuantity ?? 1) : null,
                MaxQuantity = entity.PricingMode == SiteChargePricingMode.Range ? item.MaxQuantity : null,
                PackageQuantity = entity.PricingMode == SiteChargePricingMode.Package ? item.PackageQuantity : null,
                Price = item.Price,
                SortOrder = item.SortOrder > 0 ? item.SortOrder : i,
            });
        }

        await context.SaveChangesAsync();
        return OperationResult<SiteChargeServiceAdminDto>.Success(Map(entity));
    }

    public async Task<OperationResult> DeleteAsync(int id)
    {
        var entity = await context.SiteChargeServices.FirstOrDefaultAsync(s => s.SiteChargeServiceId == id);
        if (entity == null)
            return OperationResult.Failure("سرویس یافت نشد");

        context.SiteChargeServices.Remove(entity);
        await context.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    private static string? Validate(SaveSiteChargeServiceCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان الزامی است";

        if (!Enum.IsDefined(command.Code))
            return "کد سرویس نامعتبر است";

        if (!Enum.IsDefined(command.PricingMode))
            return "حالت قیمت‌گذاری نامعتبر است";

        var prices = command.Prices ?? [];
        if (prices.Count == 0)
            return "حداقل یک ردیف قیمت لازم است";

        if (prices.Any(p => p.Price < 0))
            return "قیمت نمی‌تواند منفی باشد";

        return command.PricingMode switch
        {
            SiteChargePricingMode.Fixed => ValidateFixed(prices),
            SiteChargePricingMode.Range => ValidateRange(prices),
            SiteChargePricingMode.Package => ValidatePackage(prices),
            _ => "حالت قیمت‌گذاری نامعتبر است",
        };
    }

    private static string? ValidateFixed(List<SaveSiteChargeServicePriceItem> prices)
        => prices.Count != 1 ? "در حالت قیمت ثابت فقط یک ردیف قیمت مجاز است" : null;

    private static string? ValidateRange(List<SaveSiteChargeServicePriceItem> prices)
    {
        foreach (var p in prices)
        {
            var min = p.MinQuantity ?? 1;
            if (min < 1)
                return "حد پایین بازه باید حداقل ۱ باشد";
            if (p.MaxQuantity is < 1)
                return "حد بالای بازه نامعتبر است";
            if (p.MaxQuantity is int max && max < min)
                return "حد بالای بازه نمی‌تواند کمتر از حد پایین باشد";
        }

        var ordered = prices
            .Select(p => (Min: p.MinQuantity ?? 1, Max: p.MaxQuantity))
            .OrderBy(p => p.Min)
            .ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            var prev = ordered[i - 1];
            var curr = ordered[i];
            if (prev.Max == null)
                return "بازه بدون سقف باید آخرین بازه باشد";
            if (curr.Min <= prev.Max.Value)
                return "بازه‌های تعداد نباید هم‌پوشانی داشته باشند";
        }

        return null;
    }

    private static string? ValidatePackage(List<SaveSiteChargeServicePriceItem> prices)
    {
        if (prices.Any(p => p.PackageQuantity is null or < 1))
            return "تعداد واحد هر بسته باید حداقل ۱ باشد";

        if (prices.Select(p => p.PackageQuantity!.Value).Distinct().Count() != prices.Count)
            return "تعداد واحد بسته‌ها نباید تکراری باشد";

        return null;
    }

    private static SiteChargeServiceAdminDto Map(SiteChargeService entity)
        => new()
        {
            SiteChargeServiceId = entity.SiteChargeServiceId,
            Code = entity.Code,
            Title = entity.Title,
            Description = entity.Description,
            PricingMode = entity.PricingMode,
            IsActive = entity.IsActive,
            UpdatedAt = entity.UpdatedAt,
            Prices = entity.Prices
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.MinQuantity ?? p.PackageQuantity ?? 0)
                .Select(p => new SiteChargeServicePriceAdminDto
                {
                    SiteChargeServicePriceId = p.SiteChargeServicePriceId,
                    Title = p.Title,
                    MinQuantity = p.MinQuantity,
                    MaxQuantity = p.MaxQuantity,
                    PackageQuantity = p.PackageQuantity,
                    Price = p.Price,
                    SortOrder = p.SortOrder,
                })
                .ToList(),
        };
}
