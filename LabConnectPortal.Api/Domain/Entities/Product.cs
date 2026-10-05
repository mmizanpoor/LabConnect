using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class Product
{
    [Key]
    public Guid ProductId { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public int? BrandId { get; set; }

    [MaxLength(500)]
    public string Warranty { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    [MaxLength(500)]
    public string? FeaturedImagePath { get; set; }

    public decimal Price { get; set; }

    public bool IsNegotiablePrice { get; set; } = true;

    public bool IsUsed { get; set; }

    public int StockQuantity { get; set; } = 1;

    public decimal? DiscountPercent { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public int? ProvinceId { get; set; }
    public virtual Province? Province { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }

    public Guid? CreatedByCenterProfileId { get; set; }
    public virtual CenterProfile? CreatedByCenterProfile { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public virtual User? ApprovedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public int ViewCount { get; set; }

    public virtual Brand? Brand { get; set; }
    public virtual ICollection<ProductCategoryAssignment> CategoryAssignments { get; set; } = new List<ProductCategoryAssignment>();
    public virtual ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductExpertReview> ExpertReviews { get; set; } = new List<ProductExpertReview>();
    public virtual ICollection<ProductView> Views { get; set; } = new List<ProductView>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<ProductOrderItem> OrderItems { get; set; } = new List<ProductOrderItem>();
    public virtual ICollection<ProductResumeApplication> ResumeApplications { get; set; } = new List<ProductResumeApplication>();
}
