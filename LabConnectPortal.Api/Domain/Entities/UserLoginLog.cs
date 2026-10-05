using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class UserLoginLog
{
    [Key]
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public LoginMethod LoginMethod { get; set; }

    public bool Success { get; set; }

    [MaxLength(100)]
    public string? Username { get; set; }

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [MaxLength(300)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual User? User { get; set; }
}
