using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductOrder
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CenterProfileId { get; set; }

    public ProductOrderStatus Status { get; set; } = ProductOrderStatus.PendingPayment;

    public decimal TotalAmount { get; set; }

    public decimal ShippingCost { get; set; }

    public ShippingMethod ShippingMethod { get; set; } = ShippingMethod.Post;

    /// <summary>Stock was decremented at checkout (reserved until pay/cancel).</summary>
    public bool StockReserved { get; set; }

    [MaxLength(200)]
    public string ShippingRecipientName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TrackingCode { get; set; }

    [MaxLength(200)]
    public string? ShippingCompany { get; set; }

    /// <summary>Optional notes when the center marks the order as delivered.</summary>
    [MaxLength(1000)]
    public string? DeliveryNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime? PaidAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual CenterProfile CenterProfile { get; set; } = null!;
    public virtual ICollection<ProductOrderItem> Items { get; set; } = new List<ProductOrderItem>();
}
