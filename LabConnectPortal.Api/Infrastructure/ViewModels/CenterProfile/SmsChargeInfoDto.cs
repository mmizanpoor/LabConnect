using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class SmsChargeInfoDto
{
    public int RemainingCount { get; set; }
    public SiteChargePricingMode PricingMode { get; set; }
    public decimal? UnitPrice { get; set; }
    public List<SiteChargeServicePriceOptionDto> Prices { get; set; } = [];
}
