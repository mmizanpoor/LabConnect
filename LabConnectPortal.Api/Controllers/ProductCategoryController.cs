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
public class ProductCategoryController(IProductCatalogService productCatalogService) : ControllerBase
{
    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll([FromBody] GetProductCategoriesQuery query)
        => await productCatalogService.GetCategoriesAsync(query);

    [HttpGet("GetAllOptions")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> GetAllOptions()
        => await productCatalogService.GetAllCategoriesAsync();

    [HttpGet("GetCategories")]
    [AllowAnonymous]
    public async Task<OperationResult> GetCategories()
        => await productCatalogService.GetCategoriesLookupAsync();

    [HttpPost("GetByProductIds")]
    [AllowAnonymous]
    public async Task<OperationResult> GetByProductIds([FromBody] GetProductCategoriesByProductIdsQuery query)
        => await productCatalogService.GetProductCategoriesByProductIdsAsync(query);

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Create([FromBody] SaveProductCategoryCommand command)
        => await productCatalogService.CreateCategoryAsync(command);

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Update([FromBody] UpdateProductCategoryCommand command)
        => await productCatalogService.UpdateCategoryAsync(command);

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Delete(int productCategoryId)
        => await productCatalogService.DeleteCategoryAsync(productCategoryId);
}

[Route("[controller]")]
[ApiController]
public class ProductAttributeController(IProductCatalogService productCatalogService) : ControllerBase
{
    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll([FromBody] GetProductAttributesQuery query)
        => await productCatalogService.GetAttributesAsync(query);

    [HttpPost("GetByCategoryIds")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> GetByCategoryIds([FromBody] GetAttributesByCategoryIdsQuery query)
        => await productCatalogService.GetAttributesByCategoryIdsAsync(query);

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Create([FromBody] SaveProductAttributeCommand command)
        => await productCatalogService.CreateAttributeAsync(command);

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Update([FromBody] UpdateProductAttributeCommand command)
        => await productCatalogService.UpdateAttributeAsync(command);

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Delete(int productAttributeId)
        => await productCatalogService.DeleteAttributeAsync(productAttributeId);
}

[Route("[controller]")]
[ApiController]
public class BrandController(
    IProductCatalogService productCatalogService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll([FromBody] GetBrandsQuery query)
        => await productCatalogService.GetBrandsAsync(query);

    [HttpGet("GetAllOptions")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin, UserType.AdminLab, UserType.UserLab, UserType.Store)]
    public async Task<OperationResult> GetAllOptions()
        => await productCatalogService.GetAllBrandsAsync();

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Create([FromBody] SaveBrandCommand command)
        => await productCatalogService.CreateBrandAsync(command);

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Update([FromBody] UpdateBrandCommand command)
        => await productCatalogService.UpdateBrandAsync(command);

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Delete(int brandId)
        => await productCatalogService.DeleteBrandAsync(brandId);

    [HttpPost("UploadImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadImage(int brandId, IFormFile file)
        => await productCatalogService.UploadBrandImageAsync(brandId, file);

    [HttpPost("DeleteImage")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> DeleteImage(int brandId)
        => await productCatalogService.DeleteBrandImageAsync(brandId);

    [HttpGet("GetImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenBrandImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });
}
