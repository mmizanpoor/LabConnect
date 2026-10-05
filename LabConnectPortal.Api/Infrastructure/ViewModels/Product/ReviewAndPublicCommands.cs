using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Product;

public class ProductReviewReplyDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ProductUserReviewDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public List<ProductReviewReplyDto> Replies { get; set; } = [];
}

public class ProductReviewListItemDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
}

public class ProductReviewEligibilityDto
{
    public bool CanReview { get; set; }
    public bool HasPendingReview { get; set; }
    public Guid? OrderItemId { get; set; }
}

public class CreateProductReviewCommand
{
    public Guid OrderItemId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class ReplyToReviewCommand
{
    public Guid ReviewId { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class GetProductReviewsQuery
{
    public Guid ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetPendingProductReviewsQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ApproveProductReviewCommand
{
    public Guid ReviewId { get; set; }
}

public class RejectProductReviewCommand
{
    public Guid ReviewId { get; set; }
}

public class PublicBrandCardDto
{
    public int BrandId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
}

public class PublicCategoryCardDto
{
    public int ProductCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class PublicCategoryGroupCardDto
{
    public int ProductCategoryGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string HomePageImagePath { get; set; } = string.Empty;
}

public class PublicProductListingCardDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandTitle { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public Guid CenterProfileId { get; set; }
    public bool HasCenterLogo { get; set; }
    public string? CategoryGroupTitle { get; set; }
    public string? CategoryTitle { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsNegotiablePrice { get; set; }
    public bool IsUsed { get; set; }
    public string? ImagePath { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool? AcceptsResume { get; set; }
}

public class GetPublishedListingsQuery
{
    public string? Title { get; set; }
    public int? CategoryId { get; set; }
    public int? CategoryGroupId { get; set; }
    public int? BrandId { get; set; }
    public List<int> BrandIds { get; set; } = [];
    public int? ProvinceId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<PublicAttributeFilterItem> AttributeFilters { get; set; } = [];
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PublicAttributeFilterItem
{
    public int ProductAttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class PublicListingFiltersDto
{
    public List<PublicBrandFilterDto> Brands { get; set; } = [];
    public List<PublicCategoryGroupFilterDto> CategoryGroups { get; set; } = [];
    public List<PublicCategoryFilterDto> Categories { get; set; } = [];
    public List<PublicProvinceFilterDto> Provinces { get; set; } = [];
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

public class PublicProvinceFilterDto
{
    public int ProvinceId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class GetFilterAttributesByGroupQuery
{
    public int CategoryGroupId { get; set; }
    public int? CategoryId { get; set; }
}

public class PublicFilterAttributeDto
{
    public int ProductAttributeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string CategoryTitle { get; set; } = string.Empty;
    public AttributeFieldType? FieldType { get; set; }
}

public class PublicBrandFilterDto
{
    public int BrandId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class PublicCategoryGroupFilterDto
{
    public int ProductCategoryGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class PublicCategoryFilterDto
{
    public int ProductCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ProductCategoryGroupId { get; set; }
    public string? CategoryGroupTitle { get; set; }
}

public class SearchProductSuggestionsQuery
{
    public string Term { get; set; } = string.Empty;
    public int Limit { get; set; } = 8;
}

public class ProductSearchSuggestionDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? CategoryLabel { get; set; }
    public Guid? ProductId { get; set; }
    public int? CategoryId { get; set; }
}

public class PublicProductListingDetailDto
{
    public Guid ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandTitle { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public Guid CenterProfileId { get; set; }
    public bool HasCenterLogo { get; set; }
    public string Warranty { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal FinalPrice { get; set; }
    public bool IsNegotiablePrice { get; set; }
    public bool IsUsed { get; set; }
    public int StockQuantity { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<string> CategoryTitles { get; set; } = [];
    public List<ProductAttributeValueDto> AttributeValues { get; set; } = [];
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductExpertReviewDto> ExpertReviews { get; set; } = [];
    public List<ProductUserReviewDto> UserReviews { get; set; } = [];
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public bool AcceptsResume { get; set; }
}

public class SellerContactDto
{
    public string Phone { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
}
