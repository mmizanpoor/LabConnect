using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Feature branch test
namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductController(
    IProductCatalogService productCatalogService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> GetAll([FromBody] GetProductsQuery query)
        => await productCatalogService.GetProductsAsync(query, GetUserId());

    [HttpPost("SearchCatalog")]
    [Authorize]
    public async Task<OperationResult> SearchCatalog([FromBody] SearchCatalogQuery query)
        => await productCatalogService.SearchCatalogAsync(query, GetUserId());

    [HttpGet("GetById")]
    [Authorize]
    public async Task<OperationResult> GetById(Guid productId)
        => await productCatalogService.GetProductByIdAsync(productId, GetUserId());

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> Create([FromBody] SaveProductCommand command)
        => await productCatalogService.CreateProductAsync(command, GetUserId());

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> Update([FromBody] UpdateProductCommand command)
        => await productCatalogService.UpdateProductAsync(command, GetUserId());

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> Delete(Guid productId)
        => await productCatalogService.DeleteProductAsync(productId, GetUserId());

    [HttpPost("SubmitForApproval")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> SubmitForApproval([FromBody] ProductIdCommand command)
        => await productCatalogService.SubmitForApprovalAsync(command.ProductId, GetUserId());

    [HttpPost("Unpublish")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> Unpublish([FromBody] ProductIdCommand command)
        => await productCatalogService.UnpublishProductAsync(command.ProductId, GetUserId());

    [HttpPost("Republish")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> Republish([FromBody] ProductIdCommand command)
        => await productCatalogService.RepublishProductAsync(command.ProductId, GetUserId());

    [HttpPost("Approve")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Approve([FromBody] ProductIdCommand command)
        => await productCatalogService.ApproveProductAsync(command.ProductId, GetUserId());

    [HttpPost("Reject")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Reject([FromBody] RejectProductCommand command)
        => await productCatalogService.RejectProductAsync(command, GetUserId());

    [HttpPost("UploadImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadImage(Guid productId, IFormFile file)
        => await productCatalogService.UploadImageAsync(productId, file, GetUserId());

    [HttpPost("DeleteImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> DeleteImage([FromBody] DeleteProductImageCommand command)
        => await productCatalogService.DeleteImageAsync(command.ProductId, command.ImageId, GetUserId());

    [HttpPost("UploadFeaturedImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadFeaturedImage(Guid productId, IFormFile file)
        => await productCatalogService.UploadFeaturedImageAsync(productId, file, GetUserId());

    [HttpPost("DeleteFeaturedImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> DeleteFeaturedImage(Guid productId)
        => await productCatalogService.DeleteFeaturedImageAsync(productId, GetUserId());

    [HttpGet("ResolveProvinceFromLocation")]
    [Authorize]
    public async Task<OperationResult> ResolveProvinceFromLocation(decimal latitude, decimal longitude)
        => await productCatalogService.ResolveProvinceFromLocationAsync(latitude, longitude);

    [HttpGet("GetImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenProductImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
