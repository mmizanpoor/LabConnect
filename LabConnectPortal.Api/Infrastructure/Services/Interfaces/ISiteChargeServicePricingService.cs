using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteChargeServicePricingService
{
    Task<SiteChargeService?> GetActiveByCodeAsync(SiteChargeServiceCode code);

    Task<OperationResult<SiteChargeServicePricingDto>> GetPricingAsync(SiteChargeServiceCode code);

    /// <summary>
    /// Fixed/Range: quantity الزامی است.
    /// Package: priceId الزامی است.
    /// </summary>
    OperationResult<SiteChargeQuoteDto> Quote(
        SiteChargeService service,
        int? quantity,
        int? priceId);
}
