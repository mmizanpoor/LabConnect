using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;

public class SiteChargeServiceAdminDto
{
    public int SiteChargeServiceId { get; set; }
    public SiteChargeServiceCode Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SiteChargePricingMode PricingMode { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SiteChargeServicePriceAdminDto> Prices { get; set; } = [];
}

public class SiteChargeServicePriceAdminDto
{
    public int SiteChargeServicePriceId { get; set; }
    public string? Title { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int? PackageQuantity { get; set; }
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
}

public class SaveSiteChargeServiceCommand
{
    public int? SiteChargeServiceId { get; set; }
    public SiteChargeServiceCode Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SiteChargePricingMode PricingMode { get; set; }
    public bool IsActive { get; set; } = true;
    public List<SaveSiteChargeServicePriceItem> Prices { get; set; } = [];
}

public class SaveSiteChargeServicePriceItem
{
    public string? Title { get; set; }
    public int? MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public int? PackageQuantity { get; set; }
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
}

public class DeleteSiteChargeServiceCommand
{
    public int SiteChargeServiceId { get; set; }
}
