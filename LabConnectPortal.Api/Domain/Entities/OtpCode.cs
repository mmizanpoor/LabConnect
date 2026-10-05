using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class OtpCode
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(15)]
    public string MobileNumber { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}
