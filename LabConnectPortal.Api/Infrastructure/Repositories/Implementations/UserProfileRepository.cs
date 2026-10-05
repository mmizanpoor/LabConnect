using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class UserProfileRepository(LabConnectDbContext context) : IUserProfileRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<User?> GetUserWithResumeAsync(Guid userId, bool tracking = false)
    {
        var query = _context.Users
            .Include(u => u.Profile!)
                .ThenInclude(p => p!.Province)
            .Include(u => u.WorkExperiences)
            .Include(u => u.EducationalBackgrounds)
            .Include(u => u.UserSkills)
                .ThenInclude(s => s.Skill)
            .Include(u => u.UserLanguages)
                .ThenInclude(l => l.LanguageName)
            .Include(u => u.JobPreference!)
                .ThenInclude(j => j.PreferredProvinces)
            .Include(u => u.JobPreference!)
                .ThenInclude(j => j.JobCategories)
            .Include(u => u.JobPreference!)
                .ThenInclude(j => j.SeniorityLevels)
            .Include(u => u.JobPreference!)
                .ThenInclude(j => j.AcceptableContractTypes)
            .Where(u => u.Id == userId);

        return tracking
            ? query.FirstOrDefaultAsync()
            : query.AsNoTracking().FirstOrDefaultAsync();
    }

    public Task<UserProfile?> GetProfileWithTrackingAsync(Guid userId)
        => _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

    public Task<JobPreference?> GetJobPreferenceWithTrackingAsync(Guid userId)
        => _context.JobPreferences
            .Include(j => j.PreferredProvinces)
            .Include(j => j.JobCategories)
            .Include(j => j.SeniorityLevels)
            .Include(j => j.AcceptableContractTypes)
            .FirstOrDefaultAsync(j => j.UserId == userId);

    public Task<List<Skill>> SearchSkillsAsync(string term, int limit = 10)
        => _context.Skills.AsNoTracking()
            .Where(s => s.SkillName.Contains(term))
            .OrderBy(s => s.SkillName)
            .Take(limit)
            .ToListAsync();

    public Task<Skill?> FindSkillByNameAsync(string normalizedName)
        => _context.Skills.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SkillName.ToLower() == normalizedName);

    public Task<List<Province>> GetProvincesAsync()
        => _context.Provinces.AsNoTracking().OrderBy(p => p.ProvinceName).ToListAsync();

    public Task<List<JobCategory>> GetJobCategoriesAsync()
        => _context.JobCategories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();

    public Task<List<SalaryRange>> GetSalaryRangesAsync()
        => _context.SalaryRanges.AsNoTracking().OrderBy(s => s.SalaryRangeId).ToListAsync();

    public Task<List<LanguageName>> GetLanguageNamesAsync()
        => _context.LanguageNames.AsNoTracking().OrderBy(l => l.Name).ToListAsync();

    public async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }
    }
}
