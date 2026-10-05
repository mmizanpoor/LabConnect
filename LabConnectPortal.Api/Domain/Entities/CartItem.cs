using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class CartItem
{
    [Key]
    public Guid Id { get; set; }

    public Guid CartId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;
    public virtual Product? Product { get; set; }
}
