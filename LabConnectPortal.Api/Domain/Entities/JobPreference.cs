using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPreference
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public int? MinimumSalaryId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual SalaryRange? MinimumSalary { get; set; }
    public virtual ICollection<JobPreferenceProvince> PreferredProvinces { get; set; } = new List<JobPreferenceProvince>();
    public virtual ICollection<JobPreferenceJobCategory> JobCategories { get; set; } = new List<JobPreferenceJobCategory>();
    public virtual ICollection<JobPreferenceSeniorityLevel> SeniorityLevels { get; set; } = new List<JobPreferenceSeniorityLevel>();
    public virtual ICollection<JobPreferenceContractType> AcceptableContractTypes { get; set; } = new List<JobPreferenceContractType>();
}
