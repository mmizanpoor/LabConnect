using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Brand
{
    [Key]
    public int BrandId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    public bool ShowOnHomePage { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
