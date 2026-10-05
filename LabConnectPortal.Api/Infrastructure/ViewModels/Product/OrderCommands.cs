using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Product;

public class CartItemDto
{
    public Guid Id { get; set; }
    public CartItemType ItemType { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string CenterName { get; set; } = string.Empty;
    public Guid CenterProfileId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
}

public class CartDto
{
    public Guid? CenterProfileId { get; set; }
    public string? CenterName { get; set; }
    public decimal DefaultShippingCost { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
}

public class AddToCartCommand
{
    public Guid? ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemCommand
{
    public Guid CartItemId { get; set; }
    public int Quantity { get; set; }
}

public class CartItemIdCommand
{
    public Guid CartItemId { get; set; }
}

public class CheckoutCommand
{
    public bool ReplaceExistingCenterItems { get; set; }

    /// <summary>Optional; server falls back to user profile when empty.</summary>
    public string? ShippingRecipientName { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingPhone { get; set; }
    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Post;
}

public class ProductOrderListItemDto
{
    public Guid Id { get; set; }
    public string CenterName { get; set; } = string.Empty;
    public string BuyerAddress { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public ProductOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    /// <summary>True when completed order can be marked shipped by the center.</summary>
    public bool CanShip { get; set; }
    /// <summary>True when shipped order can be marked delivered by the center.</summary>
    public bool CanDeliver { get; set; }
    /// <summary>True when buyer can confirm delivery (order is Shipped).</summary>
    public bool CanConfirmDelivery { get; set; }
}

public class ProductOrderItemDto
{
    public Guid Id { get; set; }
    public CartItemType ItemType { get; set; }
    public Guid? ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public bool HasReview { get; set; }
    public bool CanReview { get; set; }
    public DateTime? ShippedAt { get; set; }
    public bool IsShipped { get; set; }
}

public class ProductOrderDto
{
    public Guid Id { get; set; }
    public Guid CenterProfileId { get; set; }
    public string CenterName { get; set; } = string.Empty;
    public ProductOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string ShippingRecipientName { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string? TrackingCode { get; set; }
    public string? ShippingCompany { get; set; }
    public string? DeliveryNotes { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public bool CanShip { get; set; }
    public bool CanDeliver { get; set; }
    public bool CanConfirmDelivery { get; set; }
    public List<ProductOrderItemDto> Items { get; set; } = [];
}

public class GetMyOrdersQuery
{
    public ProductOrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetCenterOrdersQuery
{
    public ProductOrderStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class OrderIdCommand
{
    public Guid OrderId { get; set; }
}

public class ShipOrderCommand
{
    public Guid OrderId { get; set; }
    public string? TrackingCode { get; set; }
    public string? ShippingCompany { get; set; }
}

public class DeliverOrderCommand
{
    public Guid OrderId { get; set; }
    public string? Notes { get; set; }
}

public class ProductOrderItemIdCommand
{
    public Guid OrderItemId { get; set; }
}

public class GetShipmentItemsQuery
{
    /// <summary>
    /// pending | shipped | all (default: pending)
    /// </summary>
    public string? ShipmentStatus { get; set; }

    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductOrderShipmentItemDto
{
    public Guid Id { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string BuyerCenterName { get; set; } = string.Empty;
    public string BuyerCenterAddress { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public bool IsShipped { get; set; }
}

public class GetAdminOrdersQuery
{
    public ProductOrderStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AdminProductOrderListItemDto
{
    public Guid Id { get; set; }
    public string CenterName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public ProductOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public ShippingMethod ShippingMethod { get; set; }
    public string? TrackingCode { get; set; }
    public string? ShippingCompany { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class CheckoutResultDto
{
    public ProductOrderDto? Order { get; set; }
}
