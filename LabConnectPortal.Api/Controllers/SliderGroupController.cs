using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class SliderGroupController(ISliderGroupService sliderGroupService) : ControllerBase
{
    [HttpPost("GetAll")]
    public async Task<OperationResult> GetAll([FromBody] GetSliderGroupsQuery query)
        => await sliderGroupService.GetAllAsync(query);

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid id)
        => await sliderGroupService.GetByIdAsync(id);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveSliderGroupCommand command)
        => await sliderGroupService.CreateAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateSliderGroupCommand command)
        => await sliderGroupService.UpdateAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid id)
        => await sliderGroupService.DeleteAsync(id);

    [HttpPost("UploadSlideImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadSlideImage(Guid sliderGroupId, IFormFile file)
        => await sliderGroupService.UploadSlideImageAsync(sliderGroupId, file);

    [HttpPost("UpdateSlide")]
    public async Task<OperationResult> UpdateSlide([FromBody] UpdateSliderSlideCommand command)
        => await sliderGroupService.UpdateSlideAsync(command);

    [HttpPost("DeleteSlide")]
    public async Task<OperationResult> DeleteSlide([FromBody] DeleteSliderSlideCommand command)
        => await sliderGroupService.DeleteSlideAsync(command);

    [HttpPost("ReorderSlides")]
    public async Task<OperationResult> ReorderSlides([FromBody] ReorderSlidesCommand command)
        => await sliderGroupService.ReorderSlidesAsync(command);

    [HttpGet("GetSlideImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlideImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => sliderGroupService.OpenSlideImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });
}
