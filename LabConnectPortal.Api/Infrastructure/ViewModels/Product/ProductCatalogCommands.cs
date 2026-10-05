using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Product;

public class ProductCategoryDto
{
    public int ProductCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool AcceptsResume { get; set; }
    public int? ProductCategoryGroupId { get; set; }
}

public class CategoryOptionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class GetProductCategoriesByProductIdsQuery
{
    public List<Guid> ProductIds { get; set; } = [];
}

public class ProductCategoryMappingDto
{
    public Guid ProductId { get; set; }
    public int ProductCategoryId { get; set; }
}

public class ProductCategoryGroupItemDto
{
    public int ProductCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class ProductCategoryGroupDto
{
    public int ProductCategoryGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? HomePageImagePath { get; set; }
    public bool ShowOnHomePage { get; set; }
    public bool HasHomePageImage { get; set; }
    public int CategoryCount { get; set; }
    public List<ProductCategoryGroupItemDto> Categories { get; set; } = [];
}

public class SaveProductCategoryGroupCommand
{
    public string Name { get; set; } = string.Empty;
    public bool ShowOnHomePage { get; set; }
    public List<int> ProductCategoryIds { get; set; } = [];
}

public class UpdateProductCategoryGroupCommand : SaveProductCategoryGroupCommand
{
    public int ProductCategoryGroupId { get; set; }
}

public class GetProductCategoryGroupsQuery
{
    public string? Name { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SaveProductCategoryCommand
{
    public string Title { get; set; } = string.Empty;
    public bool AcceptsResume { get; set; }
}

public class UpdateProductCategoryCommand : SaveProductCategoryCommand
{
    public int ProductCategoryId { get; set; }
}

public class GetProductCategoriesQuery
{
    public string? Title { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductAttributeDto
{
    public int ProductAttributeId { get; set; }
    public int ProductCategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AttributeFieldType? FieldType { get; set; }
}

public class SaveProductAttributeCommand
{
    public int ProductCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AttributeFieldType? FieldType { get; set; }
}

public class UpdateProductAttributeCommand : SaveProductAttributeCommand
{
    public int ProductAttributeId { get; set; }
}

public class GetProductAttributesQuery
{
    public int? ProductCategoryId { get; set; }
    public string? Title { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetAttributesByCategoryIdsQuery
{
    public List<int> CategoryIds { get; set; } = [];
}

public class BrandDto
{
    public int BrandId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public bool HasImage { get; set; }
    public bool ShowOnHomePage { get; set; }
}

public class SaveBrandCommand
{
    public string Title { get; set; } = string.Empty;
    public bool ShowOnHomePage { get; set; }
}

public class UpdateBrandCommand : SaveBrandCommand
{
    public int BrandId { get; set; }
}

public class GetBrandsQuery
{
    public string? Title { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductAttributeValueDto
{
    public int ProductAttributeId { get; set; }
    public string AttributeTitle { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public AttributeFieldType? FieldType { get; set; }
}

public class SaveProductAttributeValueCommand
{
    public int ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ProductExpertReviewDto
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ProductListItemDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandTitle { get; set; } = string.Empty;
    public List<string> CategoryTitles { get; set; } = [];
    public ProductStatus Status { get; set; }
    public string? FeaturedImagePath { get; set; }
    public bool HasFeaturedImage { get; set; }
    public decimal Price { get; set; }
    public bool IsNegotiablePrice { get; set; }
    public int StockQuantity { get; set; }
    public decimal? DiscountPercent { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public Guid? CreatedByCenterProfileId { get; set; }
    public string CreatedByCenterName { get; set; } = string.Empty;
    /// <summary>True when the product was defined by a site admin (Administrator/Admin).</summary>
    public bool CreatedBySiteAdmin { get; set; }
    public bool AcceptsResume { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public string BrandTitle { get; set; } = string.Empty;
    public string Warranty { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProductCategoryGroupId { get; set; }
    public string? ProductCategoryGroupName { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<string> CategoryTitles { get; set; } = [];
    public List<ProductAttributeValueDto> AttributeValues { get; set; } = [];
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductExpertReviewDto> ExpertReviews { get; set; } = [];
    public ProductStatus Status { get; set; }
    public string? FeaturedImagePath { get; set; }
    public bool HasFeaturedImage { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? CreatedByCenterProfileId { get; set; }
    public string CreatedByCenterName { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public bool CreatedBySiteAdmin { get; set; }
    public decimal Price { get; set; }
    public bool IsNegotiablePrice { get; set; } = true;
    public bool IsUsed { get; set; }
    public int StockQuantity { get; set; } = 1;
    public decimal? DiscountPercent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveProductCommand
{
    public string Title { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public string Warranty { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProductCategoryGroupId { get; set; }
    public Guid? CreatedByCenterProfileId { get; set; }
    public decimal Price { get; set; }
    public bool IsNegotiablePrice { get; set; } = true;
    public bool IsUsed { get; set; }
    public int StockQuantity { get; set; } = 1;
    public decimal? DiscountPercent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<SaveProductAttributeValueCommand> AttributeValues { get; set; } = [];
    public List<ProductExpertReviewDto> ExpertReviews { get; set; } = [];
}

public class ProductApiCreateProductCommand : SaveProductCommand
{
    public string? FeaturedImageBase64 { get; set; }
    public string? FeaturedImageFileName { get; set; }
}

public class UpdateProductCommand : SaveProductCommand
{
    public Guid ProductId { get; set; }
}

public class ProductProvinceDto
{
    public int ProvinceId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
}

public class GetProductsQuery
{
    public string? Title { get; set; }
    public string? CreatedByUserName { get; set; }
    public Guid? CreatedByCenterProfileId { get; set; }
    public string? CreatedByCenterName { get; set; }
    public int? BrandId { get; set; }
    public int? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? PendingApprovalOnly { get; set; }
    public bool? MineOnly { get; set; }
    /// <summary>
    /// When true, only listings in categories that accept resumes.
    /// When false, only listings that do not accept resumes.
    /// </summary>
    public bool? AcceptsResume { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchCatalogQuery
{
    public string? Title { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductCatalogOptionDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandTitle { get; set; } = string.Empty;
}

public class ProductApiLookupItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class ProductApiCategoryDto : ProductApiLookupItemDto
{
    public int? ProductCategoryGroupId { get; set; }
    public string? ProductCategoryGroupTitle { get; set; }
    public bool AcceptsResume { get; set; }
}

public class ProductIdCommand
{
    public Guid ProductId { get; set; }
}

public class RejectProductCommand
{
    public Guid ProductId { get; set; }
    public string? RejectionReason { get; set; }
}

public class DeleteProductImageCommand
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
}
