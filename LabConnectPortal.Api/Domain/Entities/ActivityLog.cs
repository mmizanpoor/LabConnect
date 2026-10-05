using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

public class ActivityLog
{
    [Key]
    public long Id { get; set; }

    public Guid BatchId { get; set; }

    public Guid? UserId { get; set; }

    [MaxLength(30)]
    public string UserType { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [MaxLength(64)]
    public string RecordKey { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? RecordTitle { get; set; }

    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? OldValue { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? NewValue { get; set; }

    public Guid? CenterProfileId { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}
