using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IUserProfileRepository
{
    Task<User?> GetUserWithResumeAsync(Guid userId, bool tracking = false);
    Task<UserProfile?> GetProfileWithTrackingAsync(Guid userId);
    Task<JobPreference?> GetJobPreferenceWithTrackingAsync(Guid userId);
    Task<List<Skill>> SearchSkillsAsync(string term, int limit = 10);
    Task<Skill?> FindSkillByNameAsync(string normalizedName);
    Task<List<Province>> GetProvincesAsync();
    Task<List<JobCategory>> GetJobCategoriesAsync();
    Task<List<SalaryRange>> GetSalaryRangesAsync();
    Task<List<LanguageName>> GetLanguageNamesAsync();
    Task<OperationResult> SaveChangesAsync();
}
