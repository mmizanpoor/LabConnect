using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteChargeServiceAdminService
{
    Task<OperationResult<List<SiteChargeServiceAdminDto>>> GetAllAsync();
    Task<OperationResult<SiteChargeServiceAdminDto>> SaveAsync(Guid userId, SaveSiteChargeServiceCommand command);
    Task<OperationResult> DeleteAsync(int id);
}
