using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;

public class SiteChargeServicePricingDto
{
    public SiteChargeServiceCode Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public SiteChargePricingMode PricingMode { get; set; }
    public List<SiteChargeServicePriceOptionDto> Prices { get; set; } = [];
}

public class SiteChargeServicePriceOptionDto
{
    public int SiteChargeServicePriceId { get; set; }
    public string? Title { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int? PackageQuantity { get; set; }
    public decimal Price { get; set; }
}

/// <summary>نتیجه محاسبه مبلغ قابل پرداخت برای یک درخواست شارژ.</summary>
public class SiteChargeQuoteDto
{
    public int CreditQuantity { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? SelectedPriceId { get; set; }
}
