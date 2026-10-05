using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ICartService
{
    Task<OperationResult<CartDto>> GetCartAsync(Guid userId);
    Task<OperationResult<CartDto>> AddToCartAsync(Guid userId, AddToCartCommand command);
    Task<OperationResult<CartDto>> UpdateItemAsync(Guid userId, UpdateCartItemCommand command);
    Task<OperationResult<CartDto>> RemoveItemAsync(Guid userId, Guid cartItemId);
    Task<OperationResult<CartDto>> ClearCartAsync(Guid userId);
}

public interface IProductOrderService
{
    Task<OperationResult<ProductOrderDto>> CheckoutAsync(Guid userId, CheckoutCommand command);
    Task<OperationResult<CheckoutResultDto>> CheckoutCartAsync(Guid userId, CheckoutCommand command);
    Task<OperationResult<ProductOrderDto>> ConfirmPaymentAsync(Guid userId, Guid orderId);
    Task<OperationResult<PagedResult<ProductOrderListItemDto>>> GetMyOrdersAsync(Guid userId, GetMyOrdersQuery query);
    Task<OperationResult<ProductOrderDto>> GetMyOrderByIdAsync(Guid userId, Guid orderId);
    Task<OperationResult> CancelOrderAsync(Guid userId, Guid orderId);
    Task<OperationResult<PagedResult<ProductOrderListItemDto>>> GetCenterOrdersAsync(Guid userId, GetCenterOrdersQuery query);
    Task<OperationResult<ProductOrderDto>> GetCenterOrderByIdAsync(Guid userId, Guid orderId);
    Task<OperationResult<ProductOrderDto>> MarkPaidAsync(Guid userId, Guid orderId);
    Task<OperationResult<ProductOrderDto>> CompleteAsync(Guid userId, Guid orderId);
    Task<OperationResult<ProductOrderDto>> CancelCenterOrderAsync(Guid userId, Guid orderId);
    Task<OperationResult<PagedResult<ProductOrderShipmentItemDto>>> GetShipmentItemsAsync(Guid userId, GetShipmentItemsQuery query);
    Task<OperationResult<ProductOrderShipmentItemDto>> MarkOrderItemShippedAsync(Guid userId, Guid orderItemId);
    Task<OperationResult<ProductOrderDto>> MarkOrderShippedAsync(Guid userId, ShipOrderCommand command);
    Task<OperationResult<ProductOrderDto>> MarkOrderDeliveredAsync(Guid userId, DeliverOrderCommand command);
    Task<OperationResult<ProductOrderDto>> ConfirmDeliveryAsync(Guid userId, Guid orderId);
    Task<OperationResult<PagedResult<AdminProductOrderListItemDto>>> GetAdminOrdersAsync(GetAdminOrdersQuery query);
    Task<OperationResult<ProductOrderDto>> GetAdminOrderByIdAsync(Guid orderId);
}

public interface IProductReviewService
{
    Task<OperationResult<ProductUserReviewDto>> CreateReviewAsync(Guid userId, CreateProductReviewCommand command);
    Task<OperationResult<ProductReviewReplyDto>> ReplyAsync(Guid userId, ReplyToReviewCommand command);
    Task<OperationResult<ProductReviewEligibilityDto>> GetReviewEligibilityAsync(Guid userId, Guid productId);
    Task<OperationResult<PagedResult<ProductUserReviewDto>>> GetSellerProductReviewsAsync(Guid userId, GetProductReviewsQuery query);
    Task<OperationResult<PagedResult<ProductReviewListItemDto>>> GetPendingReviewsAsync(GetPendingProductReviewsQuery query);
    Task<OperationResult> ApproveReviewAsync(Guid adminUserId, ApproveProductReviewCommand command);
    Task<OperationResult> RejectReviewAsync(RejectProductReviewCommand command);
}

public interface IPublicProductService
{
    Task<OperationResult<PagedResult<PublicProductListingCardDto>>> GetPublishedProductsAsync(GetPublishedListingsQuery query);
    Task<OperationResult<PublicProductListingDetailDto>> GetProductDetailAsync(Guid productId);
    Task<OperationResult<List<ProductSearchSuggestionDto>>> SearchSuggestionsAsync(SearchProductSuggestionsQuery query);
    Task<OperationResult<SellerContactDto>> GetSellerContactAsync(Guid productId);
    Task<OperationResult<List<PublicBrandCardDto>>> GetHomeBrandsAsync();
    Task<OperationResult<List<PublicCategoryGroupCardDto>>> GetHomeCategoriesAsync();
    Task<OperationResult<PublicListingFiltersDto>> GetListingFiltersAsync();
    Task<OperationResult<List<PublicFilterAttributeDto>>> GetFilterAttributesByGroupAsync(GetFilterAttributesByGroupQuery query);
    Task<OperationResult<List<PublicBrandFilterDto>>> GetFilterBrandsByGroupAsync(GetFilterAttributesByGroupQuery query);
    Task<OperationResult> RecordProductViewAsync(Guid productId, string? clientIp);
}
