using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ApiKey
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string KeyName { get; set; } = string.Empty;

    [MaxLength(3)]
    public string KeyPrefix { get; set; } = string.Empty;

    [MaxLength(3)]
    public string KeySuffix { get; set; } = string.Empty;

    public int KeyLength { get; set; }

    public DateTime CreateDate { get; set; }

    public bool AllowAdd { get; set; }

    public bool AllowEdit { get; set; }

    public bool AllowView { get; set; }

    public Guid CenterProfileId { get; set; }

    public CenterProfile? CenterProfile { get; set; }
}
