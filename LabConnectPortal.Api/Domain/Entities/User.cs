using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    public UserType UserType { get; set; }

    public Guid? CenterProfileId { get; set; }

    public virtual CenterProfile? CenterProfile { get; set; }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(15)]
    public string MobileNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }
    public bool MobileConfirmed { get; set; }

    [MaxLength(512)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime? LastSeenAt { get; set; }

    [MaxLength(256)]
    public string? EmailConfirmationToken { get; set; }

    public virtual UserProfile? Profile { get; set; }
    public virtual JobPreference? JobPreference { get; set; }
    public virtual ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public virtual ICollection<EducationalBackground> EducationalBackgrounds { get; set; } = new List<EducationalBackground>();
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public virtual ICollection<UserLanguage> UserLanguages { get; set; } = new List<UserLanguage>();
    public virtual ICollection<OrganizationLocation> OrganizationLocations { get; set; } = new List<OrganizationLocation>();
    public virtual ICollection<JobPostingRequest> JobPostings { get; set; } = new List<JobPostingRequest>();
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public virtual ICollection<ProductResumeApplication> ProductResumeApplications { get; set; } = new List<ProductResumeApplication>();
}
