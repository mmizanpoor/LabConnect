using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SiteVisit
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(64)]
    public string IpHash { get; set; } = string.Empty;

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}
