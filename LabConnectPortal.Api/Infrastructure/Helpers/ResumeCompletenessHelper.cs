using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;

namespace LabConnectPortal.Api.Infrastructure.Helpers;

public static class ResumeCompletenessHelper
{
    public static bool IsCompleteForApplication(User user)
        => !string.IsNullOrWhiteSpace(user.FirstName)
           && !string.IsNullOrWhiteSpace(user.LastName)
           && user.Profile != null
           && !string.IsNullOrWhiteSpace(user.Profile.JobTitle)
           && user.Profile.EmploymentStatus.HasValue;

    public static bool IsCompleteForApplication(ResumeDto resume)
        => !string.IsNullOrWhiteSpace(resume.FirstName)
           && !string.IsNullOrWhiteSpace(resume.LastName)
           && !string.IsNullOrWhiteSpace(resume.BasicInfo.JobTitle)
           && resume.BasicInfo.EmploymentStatus.HasValue;
}
