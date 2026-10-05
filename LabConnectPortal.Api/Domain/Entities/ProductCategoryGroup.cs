using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductCategoryGroup
{
    [Key]
    public int ProductCategoryGroupId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? HomePageImagePath { get; set; }

    public bool ShowOnHomePage { get; set; }

    public virtual ICollection<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
}
