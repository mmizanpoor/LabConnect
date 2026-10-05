using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IProductCatalogService
{
    Task<OperationResult<PagedResult<ProductCategoryDto>>> GetCategoriesAsync(GetProductCategoriesQuery query);
    Task<OperationResult<List<ProductCategoryDto>>> GetAllCategoriesAsync();
    Task<OperationResult<List<CategoryOptionDto>>> GetCategoriesLookupAsync();
    Task<OperationResult<List<ProductApiCategoryDto>>> GetApiCategoriesAsync();
    Task<OperationResult<List<ProductCategoryMappingDto>>> GetProductCategoriesByProductIdsAsync(
        GetProductCategoriesByProductIdsQuery query);
    Task<OperationResult<ProductCategoryDto>> CreateCategoryAsync(SaveProductCategoryCommand command);
    Task<OperationResult<ProductCategoryDto>> UpdateCategoryAsync(UpdateProductCategoryCommand command);
    Task<OperationResult> DeleteCategoryAsync(int productCategoryId);

    Task<OperationResult<PagedResult<ProductCategoryGroupDto>>> GetCategoryGroupsAsync(GetProductCategoryGroupsQuery query);
    Task<OperationResult<List<ProductCategoryGroupDto>>> GetAllCategoryGroupsAsync();
    Task<OperationResult<ProductCategoryGroupDto>> CreateCategoryGroupAsync(SaveProductCategoryGroupCommand command);
    Task<OperationResult<ProductCategoryGroupDto>> UpdateCategoryGroupAsync(UpdateProductCategoryGroupCommand command);
    Task<OperationResult> DeleteCategoryGroupAsync(int productCategoryGroupId);
    Task<OperationResult<ProductCategoryGroupDto>> UploadCategoryGroupHomePageImageAsync(int productCategoryGroupId, IFormFile file);
    Task<OperationResult<ProductCategoryGroupDto>> DeleteCategoryGroupHomePageImageAsync(int productCategoryGroupId);

    Task<OperationResult<PagedResult<ProductAttributeDto>>> GetAttributesAsync(GetProductAttributesQuery query);
    Task<OperationResult<List<ProductAttributeDto>>> GetAttributesByCategoryIdsAsync(GetAttributesByCategoryIdsQuery query);
    Task<OperationResult<List<ProductApiLookupItemDto>>> GetApiAttributesAsync(GetAttributesByCategoryIdsQuery query);
    Task<OperationResult<ProductAttributeDto>> CreateAttributeAsync(SaveProductAttributeCommand command);
    Task<OperationResult<ProductAttributeDto>> UpdateAttributeAsync(UpdateProductAttributeCommand command);
    Task<OperationResult> DeleteAttributeAsync(int productAttributeId);

    Task<OperationResult<PagedResult<BrandDto>>> GetBrandsAsync(GetBrandsQuery query);
    Task<OperationResult<List<BrandDto>>> GetAllBrandsAsync();
    Task<OperationResult<List<ProductApiLookupItemDto>>> GetApiBrandsAsync();
    Task<OperationResult<BrandDto>> CreateBrandAsync(SaveBrandCommand command);
    Task<OperationResult<BrandDto>> UpdateBrandAsync(UpdateBrandCommand command);
    Task<OperationResult> DeleteBrandAsync(int brandId);
    Task<OperationResult<BrandDto>> UploadBrandImageAsync(int brandId, IFormFile file);
    Task<OperationResult<BrandDto>> DeleteBrandImageAsync(int brandId);

    Task<OperationResult<PagedResult<ProductListItemDto>>> GetProductsAsync(GetProductsQuery query, Guid? userId = null);
    Task<OperationResult<PagedResult<ProductCatalogOptionDto>>> SearchCatalogAsync(SearchCatalogQuery query, Guid? userId = null);
    Task<OperationResult<ProductDto>> GetProductByIdAsync(Guid productId, Guid? userId = null);
    Task<OperationResult<ProductDto>> CreateProductAsync(SaveProductCommand command, Guid userId);
    Task<OperationResult<ProductDto>> CreateProductFromApiAsync(ProductApiCreateProductCommand command, Guid userId);
    Task<OperationResult<ProductDto>> UpdateProductAsync(UpdateProductCommand command, Guid userId);
    Task<OperationResult> DeleteProductAsync(Guid productId, Guid userId);
    Task<OperationResult<ProductDto>> SubmitForApprovalAsync(Guid productId, Guid userId);
    Task<OperationResult<ProductDto>> UnpublishProductAsync(Guid productId, Guid userId);
    Task<OperationResult<ProductDto>> RepublishProductAsync(Guid productId, Guid userId);
    Task<OperationResult<ProductDto>> ApproveProductAsync(Guid productId, Guid adminUserId);
    Task<OperationResult<ProductDto>> RejectProductAsync(RejectProductCommand command, Guid adminUserId);
    Task<OperationResult<ProductImageDto>> UploadImageAsync(Guid productId, IFormFile file, Guid userId);
    Task<OperationResult> DeleteImageAsync(Guid productId, Guid imageId, Guid userId);
    Task<OperationResult<ProductDto>> UploadFeaturedImageAsync(Guid productId, IFormFile file, Guid userId);
    Task<OperationResult<ProductDto>> DeleteFeaturedImageAsync(Guid productId, Guid userId);
    Task<OperationResult<ProductProvinceDto>> ResolveProvinceFromLocationAsync(decimal latitude, decimal longitude);
}
