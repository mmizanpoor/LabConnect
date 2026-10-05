using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SpecialOfferTest
{
    [Key]
    public long Id { get; set; }

    public long SpecialOfferId { get; set; }

    public long TestInfoId { get; set; }

    public decimal? Discount { get; set; }

    public int? MaxSamples { get; set; }

    public SpecialOffer SpecialOffer { get; set; } = null!;

    public TestInfo TestInfo { get; set; } = null!;
}
