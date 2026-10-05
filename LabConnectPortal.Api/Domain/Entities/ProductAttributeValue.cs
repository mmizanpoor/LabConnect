using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductAttributeValue
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int ProductAttributeId { get; set; }

    [MaxLength(500)]
    public string Value { get; set; } = string.Empty;

    public virtual Product Product { get; set; } = null!;
    public virtual ProductAttribute Attribute { get; set; } = null!;
}
