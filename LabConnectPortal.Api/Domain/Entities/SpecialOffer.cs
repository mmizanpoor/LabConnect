using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SpecialOffer
{
    [Key]
    public long Id { get; set; }

    public int LabCodeNew { get; set; }

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Summary { get; set; } = string.Empty;

    public string FullBody { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

    public ICollection<SpecialOfferTest> Tests { get; set; } = new List<SpecialOfferTest>();

    public ICollection<SpecialOfferRequest> Requests { get; set; } = new List<SpecialOfferRequest>();
}
