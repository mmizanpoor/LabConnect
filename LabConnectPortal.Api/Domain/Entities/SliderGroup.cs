using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SliderGroup
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public virtual ICollection<SliderSlide> Slides { get; set; } = new List<SliderSlide>();
}
