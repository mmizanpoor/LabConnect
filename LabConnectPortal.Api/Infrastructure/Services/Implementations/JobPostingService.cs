using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobPosting;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class JobPostingService(
    LabConnectDbContext context,
    ICenterProfileRepository centerProfileRepository) : IJobPostingService
{
    public async Task<OperationResult<PagedResult<JobPostingListItemDto>>> GetMyPostingsAsync(Guid userId, GetMyJobPostingsQuery query)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<PagedResult<JobPostingListItemDto>>.Failure(access.Error);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.JobPostingRequests
            .AsNoTracking()
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .Where(j => j.UserId == access.DataOwnerUserId);

        if (query.Status.HasValue)
            dbQuery = dbQuery.Where(j => j.Status == query.Status.Value);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(j => j.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobPostingListItemDto
            {
                JobPostingId = j.JobPostingId,
                JobCategoryName = j.JobCategory.CategoryName,
                LocationName = j.Location.LocationName,
                Status = j.Status,
                CreatedAt = j.CreatedAt,
                PublishedAt = j.PublishedAt,
            })
            .ToListAsync();

        return OperationResult<PagedResult<JobPostingListItemDto>>.Success(new PagedResult<JobPostingListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<JobPostingDto>> GetByIdAsync(Guid userId, Guid jobPostingId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var posting = await GetPostingWithDetailsAsync(jobPostingId, access.DataOwnerUserId);
        if (posting == null)
            return OperationResult<JobPostingDto>.Failure("آگهی یافت نشد");

        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        return OperationResult<JobPostingDto>.Success(MapToDto(posting, orgSummary));
    }

    public async Task<OperationResult<JobPostingDto>> CreateAsync(Guid userId, SaveJobPostingCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult<JobPostingDto>.Failure(approvalError);

        var hasLocation = await context.OrganizationLocations.AnyAsync(
            l => l.UserId == access.DataOwnerUserId);
        if (!hasLocation)
            return OperationResult<JobPostingDto>.Failure("حداقل یک شعبه باید ثبت شود");

        var validation = await ValidateSaveCommandAsync(access.DataOwnerUserId, command);
        if (validation != null)
            return OperationResult<JobPostingDto>.Failure(validation);

        var posting = new JobPostingRequest
        {
            JobPostingId = Guid.NewGuid(),
            UserId = access.DataOwnerUserId,
            Status = JobPostingStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        ApplyScalars(posting, command);
        context.JobPostingRequests.Add(posting);
        await SyncCollectionsAsync(posting, command);

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobPostingDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var created = await GetPostingWithDetailsAsync(posting.JobPostingId, access.DataOwnerUserId);
        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        return OperationResult<JobPostingDto>.Success(MapToDto(created!, orgSummary));
    }

    public async Task<OperationResult<JobPostingDto>> UpdateAsync(Guid userId, UpdateJobPostingCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult<JobPostingDto>.Failure(approvalError);

        var posting = await GetPostingWithDetailsAsync(
            command.JobPostingId, access.DataOwnerUserId, tracking: true);
        if (posting == null)
            return OperationResult<JobPostingDto>.Failure("آگهی یافت نشد");

        if (posting.Status == JobPostingStatus.Closed)
            return OperationResult<JobPostingDto>.Failure("آگهی بسته‌شده قابل ویرایش نیست");

        var validation = await ValidateSaveCommandAsync(
            access.DataOwnerUserId, command, command.JobPostingId);
        if (validation != null)
            return OperationResult<JobPostingDto>.Failure(validation);

        ApplyScalars(posting, command);
        posting.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        await SyncCollectionsAsync(posting, command);

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobPostingDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var updated = await GetPostingWithDetailsAsync(posting.JobPostingId, access.DataOwnerUserId);
        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        return OperationResult<JobPostingDto>.Success(MapToDto(updated!, orgSummary));
    }

    public async Task<OperationResult<JobPostingDto>> PublishAsync(Guid userId, Guid jobPostingId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult<JobPostingDto>.Failure(approvalError);

        var posting = await GetPostingWithDetailsAsync(
            jobPostingId, access.DataOwnerUserId, tracking: true);
        if (posting == null)
            return OperationResult<JobPostingDto>.Failure("آگهی یافت نشد");

        if (posting.Status != JobPostingStatus.Draft)
            return OperationResult<JobPostingDto>.Failure("فقط آگهی پیش‌نویس قابل انتشار است");

        var publishValidation = ValidateForPublish(posting);
        if (publishValidation != null)
            return OperationResult<JobPostingDto>.Failure(publishValidation);

        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        if (orgSummary == null || string.IsNullOrWhiteSpace(orgSummary.Name))
            return OperationResult<JobPostingDto>.Failure("نام مجموعه در پروفایل مرکز برای انتشار الزامی است");

        posting.Status = JobPostingStatus.Active;
        posting.PublishedAt = DateTime.UtcNow.ToLocalTime();
        posting.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobPostingDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<JobPostingDto>.Success(MapToDto(posting, orgSummary));
    }

    public async Task<OperationResult<JobPostingDto>> CloseAsync(Guid userId, Guid jobPostingId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult<JobPostingDto>.Failure(approvalError);

        var posting = await GetPostingWithDetailsAsync(
            jobPostingId, access.DataOwnerUserId, tracking: true);
        if (posting == null)
            return OperationResult<JobPostingDto>.Failure("آگهی یافت نشد");

        if (posting.Status != JobPostingStatus.Active)
            return OperationResult<JobPostingDto>.Failure("فقط آگهی فعال قابل بستن است");

        posting.Status = JobPostingStatus.Closed;
        posting.ClosedAt = DateTime.UtcNow.ToLocalTime();
        posting.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobPostingDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        return OperationResult<JobPostingDto>.Success(MapToDto(posting, orgSummary));
    }

    public async Task<OperationResult<JobPostingDto>> ReopenAsync(Guid userId, Guid jobPostingId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDto>.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult<JobPostingDto>.Failure(approvalError);

        var posting = await GetPostingWithDetailsAsync(
            jobPostingId, access.DataOwnerUserId, tracking: true);
        if (posting == null)
            return OperationResult<JobPostingDto>.Failure("آگهی یافت نشد");

        if (posting.Status != JobPostingStatus.Closed)
            return OperationResult<JobPostingDto>.Failure("فقط آگهی بسته‌شده قابل بازگشایی است");

        posting.Status = JobPostingStatus.Active;
        posting.ClosedAt = null;
        posting.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobPostingDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var orgSummary = await GetOrganizationSummaryAsync(access.DataOwnerUserId);
        return OperationResult<JobPostingDto>.Success(MapToDto(posting, orgSummary));
    }

    public async Task<OperationResult> DeleteAsync(Guid userId, Guid jobPostingId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var approvalError = await ValidateApprovedCenterProfileAsync(access.User!);
        if (approvalError != null)
            return OperationResult.Failure(approvalError);

        var posting = await context.JobPostingRequests
            .FirstOrDefaultAsync(j =>
                j.JobPostingId == jobPostingId && j.UserId == access.DataOwnerUserId);

        if (posting == null)
            return OperationResult.Failure("آگهی یافت نشد");

        if (posting.Status != JobPostingStatus.Draft)
            return OperationResult.Failure("فقط آگهی پیش‌نویس قابل حذف است");

        context.JobPostingRequests.Remove(posting);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<JobPostingDashboardStatsDto>> GetDashboardStatsAsync()
    {
        var stats = await BuildDashboardStatsAsync(labCode: null, ownerUserId: null);
        return OperationResult<JobPostingDashboardStatsDto>.Success(stats);
    }

    public async Task<OperationResult<JobPostingDashboardStatsDto>> GetDashboardStatsForCurrentLabAsync(Guid userId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<JobPostingDashboardStatsDto>.Failure(access.Error);

        if (access.User!.UserType == UserType.Store || await IsStoreCenterAsync(access.User))
        {
            var storeStats = await BuildDashboardStatsAsync(labCode: null, ownerUserId: access.DataOwnerUserId);
            return OperationResult<JobPostingDashboardStatsDto>.Success(storeStats);
        }

        if (!access.User.GetLabCode().HasValue)
            return OperationResult<JobPostingDashboardStatsDto>.Failure("کد آزمایشگاه (LabCode) تعریف نشده است");

        var stats = await BuildDashboardStatsAsync(access.User.GetLabCode()!.Value, ownerUserId: null);
        return OperationResult<JobPostingDashboardStatsDto>.Success(stats);
    }

    private async Task<JobPostingDashboardStatsDto> BuildDashboardStatsAsync(int? labCode, Guid? ownerUserId)
    {
        var postings = context.JobPostingRequests.AsNoTracking();
        if (ownerUserId.HasValue)
            postings = postings.Where(j => j.UserId == ownerUserId.Value);
        else if (labCode.HasValue)
            postings = ApplyLabFilter(postings, labCode.Value);

        var activeCount = await postings.CountAsync(j => j.Status == JobPostingStatus.Active);
        var expiredCount = await postings.CountAsync(j => j.Status == JobPostingStatus.Closed);

        var applications = context.JobApplications.AsNoTracking();
        if (ownerUserId.HasValue)
        {
            applications = applications.Where(a => a.JobPosting.UserId == ownerUserId.Value);
        }
        else if (labCode.HasValue)
        {
            applications = applications.Where(a =>
                (a.JobPosting.User.CenterProfile != null
                    && a.JobPosting.User.CenterProfile.LabCode == labCode.Value)
                || context.CenterProfiles.Any(cp =>
                    cp.LabCode == labCode.Value
                    && cp.OwnerUserId == a.JobPosting.UserId));
        }

        var sentApplicationsCount = await applications.CountAsync();

        return new JobPostingDashboardStatsDto
        {
            ActiveCount = activeCount,
            ExpiredCount = expiredCount,
            SentApplicationsCount = sentApplicationsCount,
        };
    }

    private async Task<bool> IsStoreCenterAsync(User user)
    {
        var profile = await ResolveCenterProfileAsync(user);
        return profile?.CenterType == CenterType.Store;
    }

    private IQueryable<JobPostingRequest> ApplyLabFilter(IQueryable<JobPostingRequest> query, int labCode)
        => query.Where(j =>
            (j.User.CenterProfile != null && j.User.CenterProfile.LabCode == labCode)
            || context.CenterProfiles.Any(cp =>
                cp.LabCode == labCode
                && cp.OwnerUserId == j.UserId));

    private async Task<JobPostingRequest?> GetPostingWithDetailsAsync(Guid jobPostingId, Guid userId, bool tracking = false)
    {
        var query = context.JobPostingRequests
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .Include(j => j.SalaryRange)
            .Include(j => j.ContractTypes)
            .Include(j => j.EssentialSkills).ThenInclude(s => s.Skill)
            .Include(j => j.PersonalTraits)
            .Include(j => j.Benefits)
            .Where(j => j.JobPostingId == jobPostingId && j.UserId == userId);

        return tracking
            ? await query.FirstOrDefaultAsync()
            : await query.AsNoTracking().FirstOrDefaultAsync();
    }

    private async Task<string?> ValidateSaveCommandAsync(Guid userId, SaveJobPostingCommand command, Guid? jobPostingId = null)
    {
        if (!await context.JobCategories.AnyAsync(c => c.JobCategoryId == command.JobCategoryId))
            return "دسته‌بندی شغلی معتبر نیست";

        if (!await context.SalaryRanges.AnyAsync(s => s.SalaryRangeId == command.SalaryRangeId))
            return "بازه حقوق معتبر نیست";

        var location = await context.OrganizationLocations
            .FirstOrDefaultAsync(l => l.LocationId == command.LocationId && l.UserId == userId);

        if (location == null)
            return "شعبه انتخاب‌شده معتبر نیست";

        if (command.MinimumWorkExperienceYears < 0)
            return "حداقل سابقه کار نمی‌تواند منفی باشد";

        var skillIds = command.EssentialSkillIds.Distinct().ToList();
        if (skillIds.Count > 0)
        {
            var validCount = await context.Skills.CountAsync(s => skillIds.Contains(s.SkillId));
            if (validCount != skillIds.Count)
                return "یک یا چند مهارت انتخاب‌شده معتبر نیست";
        }

        return null;
    }

    private static string? ValidateForPublish(JobPostingRequest posting)
    {
        if (string.IsNullOrWhiteSpace(posting.JobDescription))
            return "توضیحات شغل برای انتشار الزامی است";

        if (posting.JobCategoryId <= 0)
            return "دسته‌بندی شغلی برای انتشار الزامی است";

        if (posting.LocationId == Guid.Empty)
            return "شعبه برای انتشار الزامی است";

        if (!posting.ContractTypes.Any())
            return "حداقل یک نوع همکاری برای انتشار الزامی است";

        return null;
    }

    private static void ApplyScalars(JobPostingRequest posting, SaveJobPostingCommand command)
    {
        posting.JobCategoryId = command.JobCategoryId;
        posting.LocationId = command.LocationId;
        posting.SalaryRangeId = command.SalaryRangeId;
        posting.MinimumWorkExperienceYears = command.MinimumWorkExperienceYears;
        posting.JobDescription = command.JobDescription?.Trim() ?? string.Empty;
        posting.GenderRequirement = command.GenderRequirement;
        posting.MilitaryServiceRequirement = command.MilitaryServiceRequirement;
        posting.MinimumDegreeLevel = command.MinimumDegreeLevel;
        posting.AdditionalNotes = command.AdditionalNotes?.Trim() ?? string.Empty;
    }

    private async Task SyncCollectionsAsync(JobPostingRequest posting, SaveJobPostingCommand command)
    {
        posting.ContractTypes.Clear();
        foreach (var contractType in command.ContractTypes.Distinct())
        {
            posting.ContractTypes.Add(new JobPostingContractType
            {
                Id = Guid.NewGuid(),
                JobPostingId = posting.JobPostingId,
                ContractType = contractType,
            });
        }

        var existingSkills = await context.JobPostingEssentialSkills
            .Where(s => s.JobPostingId == posting.JobPostingId)
            .ToListAsync();
        context.JobPostingEssentialSkills.RemoveRange(existingSkills);

        foreach (var skillId in command.EssentialSkillIds.Distinct())
        {
            context.JobPostingEssentialSkills.Add(new JobPostingEssentialSkill
            {
                Id = Guid.NewGuid(),
                JobPostingId = posting.JobPostingId,
                SkillId = skillId,
            });
        }

        var existingTraits = await context.JobPostingPersonalTraits
            .Where(t => t.JobPostingId == posting.JobPostingId)
            .ToListAsync();
        context.JobPostingPersonalTraits.RemoveRange(existingTraits);

        var traitIndex = 0;
        foreach (var trait in command.RequiredPersonalTraits.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()).Distinct())
        {
            context.JobPostingPersonalTraits.Add(new JobPostingPersonalTrait
            {
                Id = Guid.NewGuid(),
                JobPostingId = posting.JobPostingId,
                TraitText = trait,
                SortOrder = traitIndex++,
            });
        }

        var existingBenefits = await context.JobPostingBenefits
            .Where(b => b.JobPostingId == posting.JobPostingId)
            .ToListAsync();
        context.JobPostingBenefits.RemoveRange(existingBenefits);

        var benefitIndex = 0;
        foreach (var benefit in command.Benefits.Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b.Trim()).Distinct())
        {
            context.JobPostingBenefits.Add(new JobPostingBenefit
            {
                Id = Guid.NewGuid(),
                JobPostingId = posting.JobPostingId,
                BenefitText = benefit,
                SortOrder = benefitIndex++,
            });
        }
    }

    private async Task<CenterProfile?> ResolveCenterProfileAsync(User user)
    {
        if (user.CenterProfileId.HasValue)
            return await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);

        if (user.UserType == UserType.AdminLab && user.GetLabCode().HasValue)
            return await centerProfileRepository.GetByLabCodeAsync(user.GetLabCode()!.Value);

        if (user.UserType == UserType.Store)
            return await centerProfileRepository.GetByOwnerUserIdAsync(user.Id);

        return null;
    }

    private async Task<string?> ValidateApprovedCenterProfileAsync(User user)
    {
        var profile = await ResolveCenterProfileAsync(user);
        if (profile == null || !profile.IsApproved)
            return "پروفایل مرکز تأیید نشده است. امکان مدیریت آگهی وجود ندارد";

        return null;
    }

    private async Task<JobPostingOrgSummaryDto?> GetOrganizationSummaryAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        var profile = await ResolveCenterProfileAsync(user);

        if (profile == null)
            return null;

        return new JobPostingOrgSummaryDto
        {
            Name = profile.Name,
            EstablishedYear = profile.EstablishedYear,
            Description = profile.Description,
            Website = profile.Website,
            EmployeeCount = profile.EmployeeCount,
            HasLogo = !string.IsNullOrWhiteSpace(profile.LogoPath),
        };
    }

    private static JobPostingDto MapToDto(JobPostingRequest posting, JobPostingOrgSummaryDto? orgSummary)
        => new()
        {
            JobPostingId = posting.JobPostingId,
            JobCategoryId = posting.JobCategoryId,
            JobCategoryName = posting.JobCategory?.CategoryName ?? string.Empty,
            LocationId = posting.LocationId,
            LocationName = posting.Location?.LocationName ?? string.Empty,
            SalaryRangeId = posting.SalaryRangeId,
            SalaryRangeName = posting.SalaryRange?.SalaryRangeDescription ?? string.Empty,
            MinimumWorkExperienceYears = posting.MinimumWorkExperienceYears,
            JobDescription = posting.JobDescription,
            ContractTypes = posting.ContractTypes.Select(c => c.ContractType).ToList(),
            RequiredPersonalTraits = posting.PersonalTraits.OrderBy(t => t.SortOrder).Select(t => t.TraitText).ToList(),
            EssentialSkillIds = posting.EssentialSkills.Select(s => s.SkillId).ToList(),
            EssentialSkillNames = posting.EssentialSkills.Select(s => s.Skill?.SkillName ?? string.Empty).ToList(),
            Benefits = posting.Benefits.OrderBy(b => b.SortOrder).Select(b => b.BenefitText).ToList(),
            GenderRequirement = posting.GenderRequirement,
            MilitaryServiceRequirement = posting.MilitaryServiceRequirement,
            MinimumDegreeLevel = posting.MinimumDegreeLevel,
            AdditionalNotes = posting.AdditionalNotes,
            Status = posting.Status,
            CreatedAt = posting.CreatedAt,
            UpdatedAt = posting.UpdatedAt,
            PublishedAt = posting.PublishedAt,
            ClosedAt = posting.ClosedAt,
            OrganizationSummary = orgSummary,
        };

    private async Task<(User? User, Guid DataOwnerUserId, string? Error)> ValidateManagerAccessAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return (null, Guid.Empty, "کاربر یافت نشد");

        if (user.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
            return (null, Guid.Empty, "این بخش فقط برای مدیر آزمایشگاه و فروشگاه است");

        if (user.UserType == UserType.UserLab)
        {
            if (!user.CenterProfileId.HasValue)
                return (null, Guid.Empty, "پروفایل مرکز برای حساب کاربری تعریف نشده است");

            var adminUserId = await context.Users.AsNoTracking()
                .Where(candidate =>
                    candidate.UserType == UserType.AdminLab &&
                    candidate.CenterProfileId == user.CenterProfileId)
                .Select(candidate => (Guid?)candidate.Id)
                .FirstOrDefaultAsync();

            return (user, adminUserId ?? user.Id, null);
        }

        return (user, user.Id, null);
    }

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }
    }
}
