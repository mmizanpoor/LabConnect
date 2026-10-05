using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductOrderItem
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? DiscountPercent { get; set; }

    public decimal LineTotal { get; set; }

    public DateTime? ShippedAt { get; set; }

    public virtual ProductOrder Order { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual ProductUserReview? UserReview { get; set; }
}
