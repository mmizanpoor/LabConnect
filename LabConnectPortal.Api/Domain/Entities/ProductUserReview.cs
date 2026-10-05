using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductUserReview
{
    [Key]
    public Guid Id { get; set; }

    public Guid OrderItemId { get; set; }

    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual ProductOrderItem OrderItem { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<ProductReviewReply> Replies { get; set; } = new List<ProductReviewReply>();
}
