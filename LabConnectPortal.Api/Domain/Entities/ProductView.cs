using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ProductView
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    [MaxLength(64)]
    public string IpHash { get; set; } = string.Empty;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual Product Product { get; set; } = null!;
}
