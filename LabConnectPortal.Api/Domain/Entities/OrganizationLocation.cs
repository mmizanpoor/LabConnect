using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class OrganizationLocation
{
    [Key]
    public Guid LocationId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string LocationName { get; set; } = string.Empty;

    public int ProvinceId { get; set; }

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
    public virtual Province Province { get; set; } = null!;
}
