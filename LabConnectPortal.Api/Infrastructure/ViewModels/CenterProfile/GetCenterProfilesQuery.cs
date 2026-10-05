using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class GetCenterProfilesQuery
{
    public string? Name { get; set; }
    public string? LabCode { get; set; }
    public string? Mobile { get; set; }
    public ProfileCompletionFilter CompletionStatus { get; set; } = ProfileCompletionFilter.All;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
