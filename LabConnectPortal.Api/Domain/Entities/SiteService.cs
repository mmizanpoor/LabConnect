using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SiteService
{
    [Key]
    public int SiteServiceId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
