using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class UserProfileService(
    IUserProfileRepository userProfileRepository,
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : IUserProfileService
{
    public async Task<OperationResult<ResumeDto>> GetMyResumeAsync(Guid userId)
    {
        var user = await userProfileRepository.GetUserWithResumeAsync(userId);
        if (user == null)
            return OperationResult<ResumeDto>.Failure("کاربر یافت نشد");

        return OperationResult<ResumeDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult<ResumeDto>> UpdateBasicInfoAsync(Guid userId, UpdateBasicInfoCommand command)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<ResumeDto>.Failure("کاربر یافت نشد");

        if (string.IsNullOrWhiteSpace(command.FirstName) || string.IsNullOrWhiteSpace(command.LastName))
            return OperationResult<ResumeDto>.Failure("نام و نام خانوادگی الزامی است");

        user.FirstName = command.FirstName.Trim();
        user.LastName = command.LastName.Trim();

        var profile = await GetOrCreateProfileAsync(userId);
        profile.JobTitle = command.JobTitle?.Trim() ?? string.Empty;
        profile.EmploymentStatus = command.EmploymentStatus;

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> UpdateAboutMeAsync(Guid userId, UpdateAboutMeCommand command)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        profile.AboutMe = command.AboutMe?.Trim() ?? string.Empty;

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoCommand command)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        profile.Email = command.Email?.Trim() ?? string.Empty;
        profile.MobilePhone = command.MobilePhone?.Trim() ?? string.Empty;
        profile.ProvinceId = command.ProvinceId;
        profile.Address = command.Address?.Trim() ?? string.Empty;
        profile.MaritalStatus = command.MaritalStatus;
        profile.BirthYear = command.BirthYear;
        profile.Gender = command.Gender;

        profile.MilitaryServiceStatus = command.Gender == Gender.Male
            ? command.MilitaryServiceStatus
            : null;

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> UpdateJobPreferenceAsync(Guid userId, UpdateJobPreferenceCommand command)
    {
        var preference = await userProfileRepository.GetJobPreferenceWithTrackingAsync(userId);
        if (preference == null)
        {
            preference = new JobPreference { UserId = userId };
            context.JobPreferences.Add(preference);
        }

        preference.MinimumSalaryId = command.MinimumSalaryId;

        SyncJobPreferenceCollections(preference, command);

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> SaveWorkExperiencesAsync(Guid userId, SaveWorkExperiencesCommand command)
    {
        var existing = await context.WorkExperiences.Where(w => w.UserId == userId).ToListAsync();
        context.WorkExperiences.RemoveRange(existing);

        foreach (var item in command.Items)
        {
            var endMonth = item.IsCurrentlyEmployed ? null : item.EndMonth;
            var endYear = item.IsCurrentlyEmployed ? null : item.EndYear;

            context.WorkExperiences.Add(new WorkExperience
            {
                WorkExperienceId = item.WorkExperienceId == Guid.Empty || item.WorkExperienceId == null
                    ? Guid.NewGuid()
                    : item.WorkExperienceId.Value,
                UserId = userId,
                JobTitle = item.JobTitle?.Trim() ?? string.Empty,
                CompanyName = item.CompanyName?.Trim() ?? string.Empty,
                StartMonth = item.StartMonth,
                StartYear = item.StartYear,
                EndMonth = endMonth,
                EndYear = endYear,
                IsCurrentlyEmployed = item.IsCurrentlyEmployed,
                JobDescription = item.JobDescription?.Trim() ?? string.Empty,
            });
        }

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> SaveEducationsAsync(Guid userId, SaveEducationsCommand command)
    {
        var existing = await context.EducationalBackgrounds.Where(e => e.UserId == userId).ToListAsync();
        context.EducationalBackgrounds.RemoveRange(existing);

        foreach (var item in command.Items)
        {
            context.EducationalBackgrounds.Add(new EducationalBackground
            {
                EducationId = item.EducationId == Guid.Empty || item.EducationId == null
                    ? Guid.NewGuid()
                    : item.EducationId.Value,
                UserId = userId,
                FieldOfStudy = item.FieldOfStudy?.Trim() ?? string.Empty,
                InstitutionName = item.InstitutionName?.Trim() ?? string.Empty,
                DegreeLevel = item.DegreeLevel,
                StartYear = item.StartYear,
                EndYear = item.IsCurrentlyStudying ? null : item.EndYear,
                IsCurrentlyStudying = item.IsCurrentlyStudying,
                Description = item.Description?.Trim() ?? string.Empty,
            });
        }

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> SaveSkillsAsync(Guid userId, SaveSkillsCommand command)
    {
        var existing = await context.UserSkills.Where(s => s.UserId == userId).ToListAsync();
        context.UserSkills.RemoveRange(existing);

        foreach (var item in command.Items)
        {
            var skillId = item.SkillId;
            if (skillId <= 0 && !string.IsNullOrWhiteSpace(item.SkillName))
            {
                var create = await CreateSkillInternalAsync(item.SkillName);
                if (create == null)
                    continue;
                skillId = create.SkillId;
            }

            if (skillId <= 0)
                continue;

            context.UserSkills.Add(new UserSkill
            {
                UserSkillId = Guid.NewGuid(),
                UserId = userId,
                SkillId = skillId,
                ProficiencyLevel = item.ProficiencyLevel,
            });
        }

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> SaveLanguagesAsync(Guid userId, SaveLanguagesCommand command)
    {
        var existing = await context.UserLanguages.Where(l => l.UserId == userId).ToListAsync();
        context.UserLanguages.RemoveRange(existing);

        foreach (var item in command.Items)
        {
            if (item.LanguageNameId <= 0)
                continue;

            context.UserLanguages.Add(new UserLanguage
            {
                UserLanguageId = Guid.NewGuid(),
                UserId = userId,
                LanguageNameId = item.LanguageNameId,
                ProficiencyLevel = item.ProficiencyLevel,
            });
        }

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> UploadPhotoAsync(Guid userId, IFormFile file)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var saveResult = await fileStorageService.SaveUserProfileFileAsync(
            userId, UserProfileFileKind.Photo, file, profile.ProfilePhotoPath);

        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ResumeDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        profile.ProfilePhotoPath = saveResult.Data;
        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> UploadResumeAsync(Guid userId, IFormFile file)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var saveResult = await fileStorageService.SaveUserProfileFileAsync(
            userId, UserProfileFileKind.Resume, file, profile.ResumeFilePath);

        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ResumeDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        profile.ResumeFilePath = saveResult.Data;
        profile.ResumeFileName = file.FileName;
        profile.ResumeFileSize = file.Length;
        profile.ResumeUploadedAt = DateTime.UtcNow.ToLocalTime();

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<OperationResult<ResumeDto>> DeleteResumeAsync(Guid userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        fileStorageService.DeleteFileIfExists(profile.ResumeFilePath);
        profile.ResumeFilePath = null;
        profile.ResumeFileName = null;
        profile.ResumeFileSize = null;
        profile.ResumeUploadedAt = null;

        var save = await userProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ResumeDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetMyResumeAsync(userId);
    }

    public async Task<(Stream? Stream, string? ContentType)> GetPhotoAsync(Guid userId)
    {
        var profile = await userProfileRepository.GetProfileWithTrackingAsync(userId);
        if (profile?.ProfilePhotoPath == null)
            return (null, null);

        return await fileStorageService.OpenUserProfileFileAsync(profile.ProfilePhotoPath);
    }

    public async Task<(Stream? Stream, string? ContentType, string? FileName)> GetResumeAsync(Guid userId)
    {
        var profile = await userProfileRepository.GetProfileWithTrackingAsync(userId);
        if (profile?.ResumeFilePath == null)
            return (null, null, null);

        var (stream, contentType) = await fileStorageService.OpenUserProfileFileAsync(profile.ResumeFilePath);
        return (stream, contentType, profile.ResumeFileName);
    }

    public async Task<OperationResult<List<ReferenceItemDto>>> GetProvincesAsync()
    {
        var items = await userProfileRepository.GetProvincesAsync();
        return OperationResult<List<ReferenceItemDto>>.Success(
            items.Select(p => new ReferenceItemDto { Id = p.ProvinceId, Name = p.ProvinceName }).ToList());
    }

    public async Task<OperationResult<List<ReferenceItemDto>>> GetJobCategoriesAsync()
    {
        var items = await userProfileRepository.GetJobCategoriesAsync();
        return OperationResult<List<ReferenceItemDto>>.Success(
            items.Select(c => new ReferenceItemDto { Id = c.JobCategoryId, Name = c.CategoryName }).ToList());
    }

    public async Task<OperationResult<List<ReferenceItemDto>>> GetSalaryRangesAsync()
    {
        var items = await userProfileRepository.GetSalaryRangesAsync();
        return OperationResult<List<ReferenceItemDto>>.Success(
            items.Select(s => new ReferenceItemDto { Id = s.SalaryRangeId, Name = s.SalaryRangeDescription }).ToList());
    }

    public async Task<OperationResult<List<ReferenceItemDto>>> GetLanguageNamesAsync()
    {
        var items = await userProfileRepository.GetLanguageNamesAsync();
        return OperationResult<List<ReferenceItemDto>>.Success(
            items.Select(l => new ReferenceItemDto { Id = l.LanguageNameId, Name = l.Name }).ToList());
    }

    public async Task<OperationResult<List<SkillSearchResultDto>>> SearchSkillsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return OperationResult<List<SkillSearchResultDto>>.Success([]);

        var skills = await userProfileRepository.SearchSkillsAsync(term.Trim(), 10);
        return OperationResult<List<SkillSearchResultDto>>.Success(
            skills.Select(s => new SkillSearchResultDto { SkillId = s.SkillId, SkillName = s.SkillName }).ToList());
    }

    public async Task<OperationResult<SkillSearchResultDto>> CreateSkillAsync(CreateSkillCommand command)
    {
        var skill = await CreateSkillInternalAsync(command.SkillName);
        if (skill == null)
            return OperationResult<SkillSearchResultDto>.Failure("نام مهارت الزامی است");

        return OperationResult<SkillSearchResultDto>.Success(
            new SkillSearchResultDto { SkillId = skill.SkillId, SkillName = skill.SkillName });
    }

    private async Task<UserProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var profile = await userProfileRepository.GetProfileWithTrackingAsync(userId);
        if (profile != null)
            return profile;

        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        profile = new UserProfile
        {
            UserId = userId,
            Email = user?.Email ?? string.Empty,
            MobilePhone = user?.MobileNumber ?? string.Empty,
            Address = user?.Address ?? string.Empty,
        };
        context.UserProfiles.Add(profile);
        return profile;
    }

    private async Task<Skill?> CreateSkillInternalAsync(string skillName)
    {
        var trimmed = skillName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return null;

        var normalized = trimmed.ToLowerInvariant();
        var existing = await userProfileRepository.FindSkillByNameAsync(normalized);
        if (existing != null)
            return existing;

        var skill = new Skill { SkillName = trimmed };
        context.Skills.Add(skill);
        await userProfileRepository.SaveChangesAsync();
        return skill;
    }

    private static void SyncJobPreferenceCollections(JobPreference preference, UpdateJobPreferenceCommand command)
    {
        preference.PreferredProvinces.Clear();
        foreach (var provinceId in command.PreferredProvinceIds.Distinct())
        {
            preference.PreferredProvinces.Add(new JobPreferenceProvince
            {
                Id = Guid.NewGuid(),
                UserId = preference.UserId,
                ProvinceId = provinceId,
            });
        }

        preference.JobCategories.Clear();
        foreach (var categoryId in command.JobCategoryIds.Distinct())
        {
            preference.JobCategories.Add(new JobPreferenceJobCategory
            {
                Id = Guid.NewGuid(),
                UserId = preference.UserId,
                JobCategoryId = categoryId,
            });
        }

        preference.SeniorityLevels.Clear();
        foreach (var level in command.SeniorityLevels.Distinct())
        {
            preference.SeniorityLevels.Add(new JobPreferenceSeniorityLevel
            {
                Id = Guid.NewGuid(),
                UserId = preference.UserId,
                SeniorityLevel = level,
            });
        }

        preference.AcceptableContractTypes.Clear();
        foreach (var contractType in command.AcceptableContractTypes.Distinct())
        {
            preference.AcceptableContractTypes.Add(new JobPreferenceContractType
            {
                Id = Guid.NewGuid(),
                UserId = preference.UserId,
                ContractType = contractType,
            });
        }
    }

    private static ResumeDto MapToDto(User user)
    {
        var profile = user.Profile;
        var latestWork = user.WorkExperiences
            .OrderByDescending(w => w.StartYear ?? 0)
            .ThenByDescending(w => w.StartMonth ?? 0)
            .FirstOrDefault();
        var latestEducation = user.EducationalBackgrounds
            .OrderByDescending(e => e.StartYear ?? 0)
            .FirstOrDefault();

        return new ResumeDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            BasicInfo = new BasicInfoDto
            {
                JobTitle = profile?.JobTitle ?? string.Empty,
                EmploymentStatus = profile?.EmploymentStatus,
                HasProfilePhoto = !string.IsNullOrEmpty(profile?.ProfilePhotoPath),
                LatestWorkSummary = latestWork == null
                    ? null
                    : $"{latestWork.JobTitle} - {latestWork.CompanyName}",
                LatestEducationSummary = latestEducation == null
                    ? null
                    : $"{latestEducation.DegreeLevel} - {latestEducation.FieldOfStudy}",
            },
            AboutMe = profile?.AboutMe ?? string.Empty,
            PersonalInfo = new PersonalInfoDto
            {
                Email = profile?.Email ?? string.Empty,
                MobilePhone = profile?.MobilePhone ?? string.Empty,
                ProvinceId = profile?.ProvinceId,
                ProvinceName = profile?.Province?.ProvinceName,
                Address = profile?.Address ?? string.Empty,
                MaritalStatus = profile?.MaritalStatus,
                BirthYear = profile?.BirthYear,
                Gender = profile?.Gender,
                MilitaryServiceStatus = profile?.MilitaryServiceStatus,
            },
            Skills = user.UserSkills.Select(s => new UserSkillDto
            {
                UserSkillId = s.UserSkillId,
                SkillId = s.SkillId,
                SkillName = s.Skill?.SkillName ?? string.Empty,
                ProficiencyLevel = s.ProficiencyLevel,
            }).ToList(),
            WorkExperiences = user.WorkExperiences.Select(w => new WorkExperienceDto
            {
                WorkExperienceId = w.WorkExperienceId,
                JobTitle = w.JobTitle,
                CompanyName = w.CompanyName,
                StartMonth = w.StartMonth,
                StartYear = w.StartYear,
                EndMonth = w.EndMonth,
                EndYear = w.EndYear,
                IsCurrentlyEmployed = w.IsCurrentlyEmployed,
                JobDescription = w.JobDescription,
            }).ToList(),
            Educations = user.EducationalBackgrounds.Select(e => new EducationDto
            {
                EducationId = e.EducationId,
                FieldOfStudy = e.FieldOfStudy,
                InstitutionName = e.InstitutionName,
                DegreeLevel = e.DegreeLevel,
                StartYear = e.StartYear,
                EndYear = e.EndYear,
                IsCurrentlyStudying = e.IsCurrentlyStudying,
                Description = e.Description,
            }).ToList(),
            Languages = user.UserLanguages.Select(l => new UserLanguageDto
            {
                UserLanguageId = l.UserLanguageId,
                LanguageNameId = l.LanguageNameId,
                LanguageName = l.LanguageName?.Name ?? string.Empty,
                ProficiencyLevel = l.ProficiencyLevel,
            }).ToList(),
            JobPreference = user.JobPreference == null ? null : new JobPreferenceDto
            {
                PreferredProvinceIds = user.JobPreference.PreferredProvinces.Select(p => p.ProvinceId).ToList(),
                JobCategoryIds = user.JobPreference.JobCategories.Select(c => c.JobCategoryId).ToList(),
                SeniorityLevels = user.JobPreference.SeniorityLevels.Select(s => s.SeniorityLevel).ToList(),
                AcceptableContractTypes = user.JobPreference.AcceptableContractTypes.Select(c => c.ContractType).ToList(),
                MinimumSalaryId = user.JobPreference.MinimumSalaryId,
            },
            ResumeFile = profile?.ResumeFilePath == null ? null : new ResumeFileDto
            {
                FileName = profile.ResumeFileName ?? string.Empty,
                FileSize = profile.ResumeFileSize ?? 0,
                UploadedAt = profile.ResumeUploadedAt,
            },
            IsCompleteForApplication = ResumeCompletenessHelper.IsCompleteForApplication(user),
        };
    }
}
