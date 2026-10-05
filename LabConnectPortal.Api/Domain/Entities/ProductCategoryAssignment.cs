using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductCategoryAssignment
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int ProductCategoryId { get; set; }

    public virtual Product Product { get; set; } = null!;
    public virtual ProductCategory Category { get; set; } = null!;
}
