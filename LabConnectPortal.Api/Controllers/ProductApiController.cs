using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductApiController(IProductCatalogService productCatalogService) : ControllerBase
{
    [HttpPost("Create")]
    [ApiKeyAuthorize(ApiKeyPermission.Add)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> Create([FromBody] ProductApiCreateProductCommand command)
        => await productCatalogService.CreateProductFromApiAsync(command, GetApiKeyContext().UserId);

    [HttpPost("Update")]
    [ApiKeyAuthorize(ApiKeyPermission.Edit)]
    public async Task<OperationResult> Update([FromBody] UpdateProductCommand command)
        => await productCatalogService.UpdateProductAsync(command, GetApiKeyContext().UserId);

    [HttpPost("GetAll")]
    [ApiKeyAuthorize(ApiKeyPermission.View)]
    public async Task<OperationResult> GetAll([FromBody] GetProductsQuery query)
        => await productCatalogService.GetProductsAsync(query, GetApiKeyContext().UserId);

    [HttpGet("GetCategories")]
    [ApiKeyAuthorize(ApiKeyPermission.Add)]
    public async Task<OperationResult> GetCategories()
        => await productCatalogService.GetApiCategoriesAsync();

    [HttpPost("GetAttributes")]
    [ApiKeyAuthorize(ApiKeyPermission.Add)]
    public async Task<OperationResult> GetAttributes([FromBody] GetAttributesByCategoryIdsQuery query)
        => await productCatalogService.GetApiAttributesAsync(query);

    [HttpGet("GetBrands")]
    [ApiKeyAuthorize(ApiKeyPermission.Add)]
    public async Task<OperationResult> GetBrands()
        => await productCatalogService.GetApiBrandsAsync();

    private ApiKeyAuthorizationDto GetApiKeyContext()
        => HttpContext.Items[ApiKeyAuthorizationFilter.ContextItemKey] as ApiKeyAuthorizationDto
           ?? throw new InvalidOperationException("API key authorization context is not available.");
}
