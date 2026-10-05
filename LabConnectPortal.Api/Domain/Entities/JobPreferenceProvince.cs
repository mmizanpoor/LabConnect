using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPreferenceProvince
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int ProvinceId { get; set; }

    public virtual JobPreference JobPreference { get; set; } = null!;
    public virtual Province Province { get; set; } = null!;
}
