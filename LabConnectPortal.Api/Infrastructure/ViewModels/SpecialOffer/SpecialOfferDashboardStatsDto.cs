namespace LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;

public class SpecialOfferDashboardStatsDto
{
    public List<SpecialOfferLabMonthlyRowDto> MonthlyOffersByLab { get; set; } = [];
}

public class SpecialOfferLabMonthlyRowDto
{
    public int LabCodeNew { get; set; }
    public string LabName { get; set; } = string.Empty;
    public int OfferCount { get; set; }
}
