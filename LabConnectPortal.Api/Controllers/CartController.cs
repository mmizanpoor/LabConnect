using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Filters;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
[SkipSystemEntity]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet("GetMyCart")]
    public async Task<OperationResult> GetMyCart()
        => await cartService.GetCartAsync(GetUserId());

    [HttpPost("AddItem")]
    public async Task<OperationResult> AddItem([FromBody] AddToCartCommand command)
        => await cartService.AddToCartAsync(GetUserId(), command);

    [HttpPost("UpdateItem")]
    public async Task<OperationResult> UpdateItem([FromBody] UpdateCartItemCommand command)
        => await cartService.UpdateItemAsync(GetUserId(), command);

    [HttpPost("RemoveItem")]
    public async Task<OperationResult> RemoveItem([FromBody] CartItemIdCommand command)
        => await cartService.RemoveItemAsync(GetUserId(), command.CartItemId);

    [HttpPost("Clear")]
    public async Task<OperationResult> Clear()
        => await cartService.ClearCartAsync(GetUserId());

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[Route("[controller]")]
[ApiController]
[Authorize]
public class ProductOrderController(IProductOrderService orderService) : ControllerBase
{
    [HttpPost("Checkout")]
    [SkipSystemEntity]
    public async Task<OperationResult> Checkout([FromBody] CheckoutCommand command)
        => await orderService.CheckoutAsync(GetUserId(), command);

    [HttpPost("CheckoutCart")]
    [SkipSystemEntity]
    public async Task<OperationResult> CheckoutCart([FromBody] CheckoutCommand command)
        => await orderService.CheckoutCartAsync(GetUserId(), command);

    [HttpPost("ConfirmPayment")]
    [SkipSystemEntity]
    public async Task<OperationResult> ConfirmPayment([FromBody] OrderIdCommand command)
        => await orderService.ConfirmPaymentAsync(GetUserId(), command.OrderId);

    [HttpPost("GetMyOrders")]
    [SkipSystemEntity]
    public async Task<OperationResult> GetMyOrders([FromBody] GetMyOrdersQuery? query)
        => await orderService.GetMyOrdersAsync(GetUserId(), query ?? new GetMyOrdersQuery());

    [HttpGet("GetMyOrderById")]
    [SkipSystemEntity]
    public async Task<OperationResult> GetMyOrderById(Guid orderId)
        => await orderService.GetMyOrderByIdAsync(GetUserId(), orderId);

    [HttpPost("Cancel")]
    [SkipSystemEntity]
    public async Task<OperationResult> Cancel([FromBody] OrderIdCommand command)
        => await orderService.CancelOrderAsync(GetUserId(), command.OrderId);

    [HttpPost("GetCenterOrders")]
    public async Task<OperationResult> GetCenterOrders([FromBody] GetCenterOrdersQuery query)
        => await orderService.GetCenterOrdersAsync(GetUserId(), query);

    [HttpGet("GetCenterOrderById")]
    public async Task<OperationResult> GetCenterOrderById(Guid orderId)
        => await orderService.GetCenterOrderByIdAsync(GetUserId(), orderId);

    [HttpPost("MarkPaid")]
    public async Task<OperationResult> MarkPaid([FromBody] OrderIdCommand command)
        => await orderService.MarkPaidAsync(GetUserId(), command.OrderId);

    [HttpPost("Complete")]
    public async Task<OperationResult> Complete([FromBody] OrderIdCommand command)
        => await orderService.CompleteAsync(GetUserId(), command.OrderId);

    [HttpPost("CancelCenterOrder")]
    public async Task<OperationResult> CancelCenterOrder([FromBody] OrderIdCommand command)
        => await orderService.CancelCenterOrderAsync(GetUserId(), command.OrderId);

    [HttpPost("GetPendingShipmentItems")]
    public async Task<OperationResult> GetPendingShipmentItems([FromBody] GetShipmentItemsQuery? query)
        => await orderService.GetShipmentItemsAsync(
            GetUserId(),
            query ?? new GetShipmentItemsQuery { ShipmentStatus = "pending" });

    [HttpPost("GetShipmentItems")]
    public async Task<OperationResult> GetShipmentItems([FromBody] GetShipmentItemsQuery query)
        => await orderService.GetShipmentItemsAsync(GetUserId(), query);

    [HttpPost("MarkOrderItemShipped")]
    public async Task<OperationResult> MarkOrderItemShipped([FromBody] ProductOrderItemIdCommand command)
        => await orderService.MarkOrderItemShippedAsync(GetUserId(), command.OrderItemId);

    [HttpPost("MarkOrderShipped")]
    public async Task<OperationResult> MarkOrderShipped([FromBody] ShipOrderCommand command)
        => await orderService.MarkOrderShippedAsync(GetUserId(), command);

    [HttpPost("MarkOrderDelivered")]
    public async Task<OperationResult> MarkOrderDelivered([FromBody] DeliverOrderCommand command)
        => await orderService.MarkOrderDeliveredAsync(GetUserId(), command);

    [HttpPost("ConfirmDelivery")]
    [SkipSystemEntity]
    public async Task<OperationResult> ConfirmDelivery([FromBody] OrderIdCommand command)
        => await orderService.ConfirmDeliveryAsync(GetUserId(), command.OrderId);

    [HttpPost("GetAdminOrders")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAdminOrders([FromBody] GetAdminOrdersQuery query)
        => await orderService.GetAdminOrdersAsync(query);

    [HttpGet("GetAdminOrderById")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAdminOrderById(Guid orderId)
        => await orderService.GetAdminOrderByIdAsync(orderId);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[Route("[controller]")]
[ApiController]
[Authorize]
public class ProductReviewController(IProductReviewService reviewService) : ControllerBase
{
    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] CreateProductReviewCommand command)
        => await reviewService.CreateReviewAsync(GetUserId(), command);

    [HttpPost("Reply")]
    public async Task<OperationResult> Reply([FromBody] ReplyToReviewCommand command)
        => await reviewService.ReplyAsync(GetUserId(), command);

    [HttpGet("GetReviewEligibility")]
    public async Task<OperationResult> GetReviewEligibility(Guid productId)
        => await reviewService.GetReviewEligibilityAsync(GetUserId(), productId);

    [HttpPost("GetSellerProductReviews")]
    public async Task<OperationResult> GetSellerProductReviews([FromBody] GetProductReviewsQuery query)
        => await reviewService.GetSellerProductReviewsAsync(GetUserId(), query);

    [HttpPost("GetPendingReviews")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> GetPendingReviews([FromBody] GetPendingProductReviewsQuery query)
        => await reviewService.GetPendingReviewsAsync(query);

    [HttpPost("Approve")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> Approve([FromBody] ApproveProductReviewCommand command)
        => await reviewService.ApproveReviewAsync(GetUserId(), command);

    [HttpPost("Reject")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> Reject([FromBody] RejectProductReviewCommand command)
        => await reviewService.RejectReviewAsync(command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[Route("[controller]")]
[ApiController]
public class PublicProductController(IPublicProductService publicProductService, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpPost("GetPublishedListings")]
    [AllowAnonymous]
    public async Task<OperationResult> GetPublishedListings([FromBody] GetPublishedListingsQuery query)
        => await publicProductService.GetPublishedProductsAsync(query);

    [HttpGet("GetListingDetail")]
    [AllowAnonymous]
    public async Task<OperationResult> GetListingDetail(Guid productId)
        => await publicProductService.GetProductDetailAsync(productId);

    [HttpGet("SearchSuggestions")]
    [AllowAnonymous]
    public async Task<OperationResult> SearchSuggestions([FromQuery] string term, [FromQuery] int limit = 8)
        => await publicProductService.SearchSuggestionsAsync(new SearchProductSuggestionsQuery { Term = term, Limit = limit });

    [HttpGet("GetSellerContact")]
    [Authorize]
    public async Task<OperationResult> GetSellerContact(Guid productId)
        => await publicProductService.GetSellerContactAsync(productId);

    [HttpPost("RecordView")]
    [AllowAnonymous]
    public async Task<OperationResult> RecordView(Guid productId)
    {
        var clientIp = ClientIpHelper.ResolveClientIp(httpContextAccessor.HttpContext);
        return await publicProductService.RecordProductViewAsync(productId, clientIp);
    }

    [HttpGet("GetListingFilters")]
    [AllowAnonymous]
    public async Task<OperationResult> GetListingFilters()
        => await publicProductService.GetListingFiltersAsync();

    [HttpGet("GetFilterAttributes")]
    [AllowAnonymous]
    public async Task<OperationResult> GetFilterAttributes(
        [FromQuery] int categoryGroupId,
        [FromQuery] int? categoryId = null)
        => await publicProductService.GetFilterAttributesByGroupAsync(
            new GetFilterAttributesByGroupQuery
            {
                CategoryGroupId = categoryGroupId,
                CategoryId = categoryId,
            });

    [HttpGet("GetFilterBrands")]
    [AllowAnonymous]
    public async Task<OperationResult> GetFilterBrands(
        [FromQuery] int categoryGroupId,
        [FromQuery] int? categoryId = null)
        => await publicProductService.GetFilterBrandsByGroupAsync(
            new GetFilterAttributesByGroupQuery
            {
                CategoryGroupId = categoryGroupId,
                CategoryId = categoryId,
            });
}

[Route("[controller]")]
[ApiController]
public class PublicBrandController(IPublicProductService publicProductService) : ControllerBase
{
    [HttpGet("GetHomeBrands")]
    [AllowAnonymous]
    public async Task<OperationResult> GetHomeBrands()
        => await publicProductService.GetHomeBrandsAsync();
}

[Route("[controller]")]
[ApiController]
public class PublicCategoryController(IPublicProductService publicProductService) : ControllerBase
{
    [HttpGet("GetHomeCategories")]
    [AllowAnonymous]
    public async Task<OperationResult> GetHomeCategories()
        => await publicProductService.GetHomeCategoriesAsync();
}
