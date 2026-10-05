using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class UserProfile
{
    [Key]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    public EmploymentStatus? EmploymentStatus { get; set; }

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15)]
    public string MobilePhone { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public MaritalStatus? MaritalStatus { get; set; }

    public int? BirthYear { get; set; }

    public Gender? Gender { get; set; }

    public MilitaryServiceStatus? MilitaryServiceStatus { get; set; }

    [MaxLength(4000)]
    public string AboutMe { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfilePhotoPath { get; set; }

    [MaxLength(500)]
    public string? ResumeFilePath { get; set; }

    [MaxLength(260)]
    public string? ResumeFileName { get; set; }

    public long? ResumeFileSize { get; set; }

    public DateTime? ResumeUploadedAt { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Province? Province { get; set; }
}
