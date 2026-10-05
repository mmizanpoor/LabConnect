using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class KitGroup
{
    public long Id { get; set; }

    public int LabCodeNew { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}
