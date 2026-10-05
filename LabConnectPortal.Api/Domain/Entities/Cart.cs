using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Cart
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual User User { get; set; } = null!;
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
