using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SalaryRange
{
    [Key]
    public int SalaryRangeId { get; set; }

    [MaxLength(200)]
    public string SalaryRangeDescription { get; set; } = string.Empty;
}
