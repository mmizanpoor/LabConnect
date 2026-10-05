using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Advertisement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class AdvertisementController(
    IAdvertisementService advertisementService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await advertisementService.GetAllAsync();

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid advertisementId)
        => await advertisementService.GetByIdAsync(advertisementId);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveAdvertisementCommand command)
        => await advertisementService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateAdvertisementCommand command)
        => await advertisementService.UpdateAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid advertisementId)
        => await advertisementService.DeleteAsync(advertisementId);

    [HttpPost("UploadImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadImage(Guid advertisementId, IFormFile file)
        => await advertisementService.UploadImageAsync(advertisementId, file);

    [HttpPost("DeleteImage")]
    public async Task<OperationResult> DeleteImage(Guid advertisementId)
        => await advertisementService.DeleteImageAsync(advertisementId);

    [HttpGet("GetImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenAdvertisementImageAsync(path),
            path,
            w);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[Route("[controller]")]
[ApiController]
public class PublicAdvertisementController(IPublicAdvertisementService publicAdvertisementService) : ControllerBase
{
    [HttpGet("GetActiveForHome")]
    [AllowAnonymous]
    public async Task<OperationResult> GetActiveForHome(int pageSize = 4)
        => await publicAdvertisementService.GetActiveForHomeAsync(pageSize);
}
