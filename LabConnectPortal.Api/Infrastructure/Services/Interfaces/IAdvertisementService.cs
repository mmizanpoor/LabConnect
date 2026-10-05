using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Advertisement;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IAdvertisementService
{
    Task<OperationResult<List<AdvertisementDto>>> GetAllAsync();
    Task<OperationResult<AdvertisementDto>> GetByIdAsync(Guid advertisementId);
    Task<OperationResult<AdvertisementDto>> CreateAsync(Guid userId, SaveAdvertisementCommand command);
    Task<OperationResult<AdvertisementDto>> UpdateAsync(UpdateAdvertisementCommand command);
    Task<OperationResult> DeleteAsync(Guid advertisementId);
    Task<OperationResult<AdvertisementDto>> UploadImageAsync(Guid advertisementId, IFormFile file);
    Task<OperationResult<AdvertisementDto>> DeleteImageAsync(Guid advertisementId);
}

public interface IPublicAdvertisementService
{
    Task<OperationResult<List<PublicAdvertisementCardDto>>> GetActiveForHomeAsync(int pageSize = 4);
}
