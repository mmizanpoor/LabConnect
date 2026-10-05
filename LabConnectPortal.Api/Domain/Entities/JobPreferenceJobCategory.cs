using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPreferenceJobCategory
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int JobCategoryId { get; set; }

    public virtual JobPreference JobPreference { get; set; } = null!;
    public virtual JobCategory JobCategory { get; set; } = null!;
}
