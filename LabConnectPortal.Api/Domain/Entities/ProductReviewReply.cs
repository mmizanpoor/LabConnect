using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductReviewReply
{
    [Key]
    public Guid Id { get; set; }

    public Guid ReviewId { get; set; }

    public Guid UserId { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual ProductUserReview Review { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
