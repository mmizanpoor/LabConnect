using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

public class SliderSlide
{
    [Key]
    public Guid Id { get; set; }

    public Guid SliderGroupId { get; set; }

    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? LinkUrl { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    [ForeignKey(nameof(SliderGroupId))]
    public virtual SliderGroup SliderGroup { get; set; } = null!;
}
