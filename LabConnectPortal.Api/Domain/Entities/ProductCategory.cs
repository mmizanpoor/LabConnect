using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductCategory
{
    [Key]
    public int ProductCategoryId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public bool AcceptsResume { get; set; }

    public int? ProductCategoryGroupId { get; set; }
    public virtual ProductCategoryGroup? CategoryGroup { get; set; }

    public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<ProductCategoryAssignment> ProductAssignments { get; set; } = new List<ProductCategoryAssignment>();
}
