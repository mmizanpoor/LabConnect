using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteService;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteServiceService
{
    Task<OperationResult<List<SiteServiceDto>>> GetAllAsync();
    Task<OperationResult<SiteServiceDto>> GetByIdAsync(int id);
    Task<OperationResult<SiteServiceDto>> CreateAsync(SaveSiteServiceCommand command);
    Task<OperationResult<SiteServiceDto>> UpdateAsync(UpdateSiteServiceCommand command);
    Task<OperationResult<SiteServiceDto>> UploadImageAsync(int id, IFormFile file);
    Task<OperationResult<SiteServiceDto>> DeleteImageAsync(int id);
    Task<OperationResult> DeleteAsync(int id);
}

public interface IPublicSiteServiceService
{
    Task<OperationResult<List<SiteServiceDto>>> GetActiveAsync();
}
