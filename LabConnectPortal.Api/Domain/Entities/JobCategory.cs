using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobCategory
{
    [Key]
    public int JobCategoryId { get; set; }

    [MaxLength(200)]
    public string CategoryName { get; set; } = string.Empty;
}
