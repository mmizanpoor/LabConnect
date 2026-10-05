using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SiteUsefulLink
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
