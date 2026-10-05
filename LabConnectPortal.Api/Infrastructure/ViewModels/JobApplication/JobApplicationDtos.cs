using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.JobApplication;

public class SubmitJobApplicationCommand
{
    public Guid JobPostingId { get; set; }
}

public class GetApplicationsForPostingQuery
{
    public Guid JobPostingId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ApproveJobApplicationCommand
{
    public Guid JobApplicationId { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
    public DateTime? VisitScheduledAt { get; set; }
    public string? VisitLocation { get; set; }
}

public class RejectJobApplicationCommand
{
    public Guid JobApplicationId { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
}

public class JobApplicationDto
{
    public Guid JobApplicationId { get; set; }
    public Guid JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string ApplicantMobileNumber { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
    public DateTime? VisitScheduledAt { get; set; }
    public string? VisitLocation { get; set; }
}

public class MyJobApplicationListItemDto
{
    public Guid JobApplicationId { get; set; }
    public Guid JobPostingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
