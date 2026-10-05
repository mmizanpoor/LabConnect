using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class ContentGroup
{
    [Key]
    public int ContentGroupId { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public virtual ICollection<ContentPost> Posts { get; set; } = new List<ContentPost>();
}
