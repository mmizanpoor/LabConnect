using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteChargeServicePricingService(LabConnectDbContext context) : ISiteChargeServicePricingService
{
    public async Task<SiteChargeService?> GetActiveByCodeAsync(SiteChargeServiceCode code)
    {
        return await context.SiteChargeServices
            .AsNoTracking()
            .Include(s => s.Prices)
            .FirstOrDefaultAsync(s => s.Code == code && s.IsActive);
    }

    public async Task<OperationResult<SiteChargeServicePricingDto>> GetPricingAsync(SiteChargeServiceCode code)
    {
        var service = await GetActiveByCodeAsync(code);
        if (service == null)
            return OperationResult<SiteChargeServicePricingDto>.Failure("سرویس یافت نشد یا غیرفعال است");

        return OperationResult<SiteChargeServicePricingDto>.Success(MapPricing(service));
    }

    public OperationResult<SiteChargeQuoteDto> Quote(
        SiteChargeService service,
        int? quantity,
        int? priceId)
    {
        var prices = service.Prices.OrderBy(p => p.SortOrder).ThenBy(p => p.MinQuantity ?? p.PackageQuantity ?? 0).ToList();
        if (prices.Count == 0)
            return OperationResult<SiteChargeQuoteDto>.Failure("تعرفه‌ای برای این سرویس تعریف نشده است");

        return service.PricingMode switch
        {
            SiteChargePricingMode.Fixed => QuoteFixed(prices, quantity),
            SiteChargePricingMode.Range => QuoteRange(prices, quantity),
            SiteChargePricingMode.Package => QuotePackage(prices, priceId),
            _ => OperationResult<SiteChargeQuoteDto>.Failure("حالت قیمت‌گذاری نامعتبر است"),
        };
    }

    private static OperationResult<SiteChargeQuoteDto> QuoteFixed(
        IReadOnlyList<SiteChargeServicePrice> prices,
        int? quantity)
    {
        if (quantity is null or < 1)
            return OperationResult<SiteChargeQuoteDto>.Failure("تعداد باید حداقل ۱ باشد");

        var unitPrice = prices[0].Price;
        return OperationResult<SiteChargeQuoteDto>.Success(new SiteChargeQuoteDto
        {
            CreditQuantity = quantity.Value,
            UnitPrice = unitPrice,
            PayableAmount = unitPrice * quantity.Value,
            SelectedPriceId = prices[0].SiteChargeServicePriceId,
        });
    }

    private static OperationResult<SiteChargeQuoteDto> QuoteRange(
        IReadOnlyList<SiteChargeServicePrice> prices,
        int? quantity)
    {
        if (quantity is null or < 1)
            return OperationResult<SiteChargeQuoteDto>.Failure("تعداد باید حداقل ۱ باشد");

        var tier = prices.FirstOrDefault(p =>
            quantity.Value >= (p.MinQuantity ?? 1) &&
            (p.MaxQuantity == null || quantity.Value <= p.MaxQuantity.Value));

        if (tier == null)
            return OperationResult<SiteChargeQuoteDto>.Failure("برای این تعداد تعرفه‌ای تعریف نشده است");

        return OperationResult<SiteChargeQuoteDto>.Success(new SiteChargeQuoteDto
        {
            CreditQuantity = quantity.Value,
            UnitPrice = tier.Price,
            PayableAmount = tier.Price * quantity.Value,
            SelectedPriceId = tier.SiteChargeServicePriceId,
        });
    }

    private static OperationResult<SiteChargeQuoteDto> QuotePackage(
        IReadOnlyList<SiteChargeServicePrice> prices,
        int? priceId)
    {
        if (priceId is null or < 1)
            return OperationResult<SiteChargeQuoteDto>.Failure("بسته مورد نظر را انتخاب کنید");

        var package = prices.FirstOrDefault(p => p.SiteChargeServicePriceId == priceId.Value);
        if (package == null || package.PackageQuantity is null or < 1)
            return OperationResult<SiteChargeQuoteDto>.Failure("بسته انتخاب‌شده معتبر نیست");

        return OperationResult<SiteChargeQuoteDto>.Success(new SiteChargeQuoteDto
        {
            CreditQuantity = package.PackageQuantity.Value,
            UnitPrice = null,
            PayableAmount = package.Price,
            SelectedPriceId = package.SiteChargeServicePriceId,
        });
    }

    private static SiteChargeServicePricingDto MapPricing(SiteChargeService service)
        => new()
        {
            Code = service.Code,
            Title = service.Title,
            PricingMode = service.PricingMode,
            Prices = service.Prices
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.MinQuantity ?? p.PackageQuantity ?? 0)
                .Select(p => new SiteChargeServicePriceOptionDto
                {
                    SiteChargeServicePriceId = p.SiteChargeServicePriceId,
                    Title = p.Title,
                    MinQuantity = p.MinQuantity,
                    MaxQuantity = p.MaxQuantity,
                    PackageQuantity = p.PackageQuantity,
                    Price = p.Price,
                })
                .ToList(),
        };
}
