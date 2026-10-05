using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductAttribute
{
    [Key]
    public int ProductAttributeId { get; set; }

    public int ProductCategoryId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Null = free-text attribute.</summary>
    public AttributeFieldType? FieldType { get; set; }

    public virtual ProductCategory Category { get; set; } = null!;
    public virtual ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}
