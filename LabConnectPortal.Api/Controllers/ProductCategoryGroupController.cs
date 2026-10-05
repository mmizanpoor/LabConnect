using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductCategoryGroupController(
    IProductCatalogService productCatalogService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpGet("GetAllOptions")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> GetAllOptions()
        => await productCatalogService.GetAllCategoryGroupsAsync();

    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll([FromBody] GetProductCategoryGroupsQuery query)
        => await productCatalogService.GetCategoryGroupsAsync(query);

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Create([FromBody] SaveProductCategoryGroupCommand command)
        => await productCatalogService.CreateCategoryGroupAsync(command);

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Update([FromBody] UpdateProductCategoryGroupCommand command)
        => await productCatalogService.UpdateCategoryGroupAsync(command);

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Delete(int productCategoryGroupId)
        => await productCatalogService.DeleteCategoryGroupAsync(productCategoryGroupId);

    [HttpPost("UploadHomePageImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadHomePageImage(int productCategoryGroupId, IFormFile file)
        => await productCatalogService.UploadCategoryGroupHomePageImageAsync(productCategoryGroupId, file);

    [HttpPost("DeleteHomePageImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> DeleteHomePageImage(int productCategoryGroupId)
        => await productCatalogService.DeleteCategoryGroupHomePageImageAsync(productCategoryGroupId);

    [HttpGet("GetHomePageImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetHomePageImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenCategoryGroupHomePageImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });
}
