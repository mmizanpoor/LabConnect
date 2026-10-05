using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

/// <summary>
/// Listing registration fee per product category.
/// </summary>
public class ProductCategoryPrice
{
    [Key]
    public int ProductCategoryPriceId { get; set; }

    public int ProductCategoryId { get; set; }

    public decimal Price { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual User? UpdatedByUser { get; set; }
}
