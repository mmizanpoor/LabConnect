namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

public class LabAgreementStatsDto
{
    public int ActiveCount { get; set; }
    public int ExpiredCount { get; set; }
    public int PendingCount { get; set; }
}

public class LabAgreementLabStatsDto
{
    public LabAgreementStatsDto Sent { get; set; } = new();
    public LabAgreementStatsDto Received { get; set; } = new();
}
