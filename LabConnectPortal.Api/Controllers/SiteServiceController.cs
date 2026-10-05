using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class SiteServiceController(
    ISiteServiceService siteServiceService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await siteServiceService.GetAllAsync();

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(int id)
        => await siteServiceService.GetByIdAsync(id);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveSiteServiceCommand command)
        => await siteServiceService.CreateAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateSiteServiceCommand command)
        => await siteServiceService.UpdateAsync(command);

    [HttpPost("UploadImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadImage(int id, IFormFile file)
        => await siteServiceService.UploadImageAsync(id, file);

    [HttpPost("DeleteImage")]
    public async Task<OperationResult> DeleteImage(int id)
        => await siteServiceService.DeleteImageAsync(id);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(int id)
        => await siteServiceService.DeleteAsync(id);

    [HttpGet("GetImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenSiteServiceImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });
}

