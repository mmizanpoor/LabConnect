using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductImage
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public virtual Product Product { get; set; } = null!;
}
