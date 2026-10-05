using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteSettingsService
{
    Task<OperationResult<SiteSettingsDto>> GetAsync();
    Task<OperationResult<SiteSettingsPublicDto>> GetPublicAsync();
    Task<OperationResult<SiteSettingsDto>> UpdateAsync(UpdateSiteSettingsCommand command);
    Task<OperationResult<SiteSettingsDto>> UploadLogoAsync(IFormFile file);
    Task<OperationResult<SiteSettingsDto>> UploadFooterLogoAsync(IFormFile file);
    Task<(Stream? Stream, string? ContentType)> OpenLogoAsync();
    Task<(Stream? Stream, string? ContentType)> OpenFooterLogoAsync();
}

public interface ISliderGroupService
{
    Task<OperationResult<PagedResult<SliderGroupListItemDto>>> GetAllAsync(GetSliderGroupsQuery query);
    Task<OperationResult<SliderGroupDto>> GetByIdAsync(Guid id);
    Task<OperationResult<SliderGroupDto>> CreateAsync(SaveSliderGroupCommand command);
    Task<OperationResult<SliderGroupDto>> UpdateAsync(UpdateSliderGroupCommand command);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<SliderSlideDto>> UploadSlideImageAsync(Guid sliderGroupId, IFormFile file);
    Task<OperationResult<SliderSlideDto>> UpdateSlideAsync(UpdateSliderSlideCommand command);
    Task<OperationResult> DeleteSlideAsync(DeleteSliderSlideCommand command);
    Task<OperationResult> ReorderSlidesAsync(ReorderSlidesCommand command);
    Task<(Stream? Stream, string? ContentType)> OpenSlideImageAsync(string path);
    Task<OperationResult<List<ActiveSliderSlideDto>>> GetActiveSlidesAsync();
}
