using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SsoAuth
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public DateTime? Date { get; set; }

    [MaxLength(200)]
    public string? PartnerName { get; set; }

    [MaxLength(50)]
    public string? PartnerPhone { get; set; }

    public DateTime? ExpireDateSup { get; set; }
}
