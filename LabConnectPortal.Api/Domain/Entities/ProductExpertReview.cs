using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductExpertReview
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
