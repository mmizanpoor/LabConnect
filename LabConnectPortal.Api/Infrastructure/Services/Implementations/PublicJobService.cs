using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.PublicJob;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class PublicJobService(
    LabConnectDbContext context,
    ICenterProfileRepository centerProfileRepository) : IPublicJobService
{
    public async Task<OperationResult<PagedResult<PublicJobPostingCardDto>>> GetActivePostingsAsync(GetActivePostingsQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.JobPostingRequests
            .AsNoTracking()
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .ThenInclude(l => l.Province)
            .Include(j => j.SalaryRange)
            .Include(j => j.ContractTypes)
            .Include(j => j.User)
            .Where(j => j.Status == JobPostingStatus.Active);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            dbQuery = dbQuery.Where(j =>
                j.JobDescription.Contains(term) ||
                (j.JobCategory != null && j.JobCategory.CategoryName.Contains(term)) ||
                (j.Location != null && j.Location.LocationName.Contains(term)) ||
                (j.Location != null && j.Location.Province != null && j.Location.Province.ProvinceName.Contains(term)));
        }

        if (query.ProvinceIds.Count > 0)
        {
            dbQuery = dbQuery.Where(j =>
                j.Location != null && query.ProvinceIds.Contains(j.Location.ProvinceId));
        }

        if (query.SalaryRangeIds.Count > 0)
            dbQuery = dbQuery.Where(j => query.SalaryRangeIds.Contains(j.SalaryRangeId));

        if (query.ContractTypes.Count > 0)
        {
            dbQuery = dbQuery.Where(j =>
                j.ContractTypes.Any(c => query.ContractTypes.Contains(c.ContractType)));
        }

        if (query.GenderRequirements.Count > 0)
            dbQuery = dbQuery.Where(j => query.GenderRequirements.Contains(j.GenderRequirement));

        if (query.MinimumDegreeLevels.Count > 0)
            dbQuery = dbQuery.Where(j => query.MinimumDegreeLevels.Contains(j.MinimumDegreeLevel));

        var totalCount = await dbQuery.CountAsync();
        var postings = await dbQuery
            .OrderByDescending(j => j.PublishedAt ?? j.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<PublicJobPostingCardDto>();
        foreach (var posting in postings)
        {
            var orgSummary = await GetOrganizationSummaryAsync(posting.UserId, posting.User);
            items.Add(new PublicJobPostingCardDto
            {
                JobPostingId = posting.JobPostingId,
                Title = BuildTitle(posting),
                OrganizationName = orgSummary?.Name ?? string.Empty,
                ProvinceName = posting.Location?.Province?.ProvinceName ?? string.Empty,
                LocationName = posting.Location?.LocationName ?? string.Empty,
                ContractTypes = posting.ContractTypes.Select(c => c.ContractType).ToList(),
                SalaryRangeName = posting.SalaryRange?.SalaryRangeDescription ?? string.Empty,
                PublishedAt = posting.PublishedAt,
                HasOrganizationLogo = orgSummary?.HasLogo ?? false,
            });
        }

        return OperationResult<PagedResult<PublicJobPostingCardDto>>.Success(new PagedResult<PublicJobPostingCardDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<PublicJobPostingFiltersDto>> GetPostingFiltersAsync()
    {
        var provinces = await context.Provinces.AsNoTracking()
            .OrderBy(p => p.ProvinceName)
            .Select(p => new PublicJobProvinceFilterDto
            {
                ProvinceId = p.ProvinceId,
                Name = p.ProvinceName,
            })
            .ToListAsync();

        var salaryRanges = await context.SalaryRanges.AsNoTracking()
            .OrderBy(s => s.SalaryRangeId)
            .Select(s => new PublicJobSalaryFilterDto
            {
                SalaryRangeId = s.SalaryRangeId,
                Name = s.SalaryRangeDescription,
            })
            .ToListAsync();

        return OperationResult<PublicJobPostingFiltersDto>.Success(new PublicJobPostingFiltersDto
        {
            Provinces = provinces,
            SalaryRanges = salaryRanges,
            ContractTypes = Enum.GetValues<ContractType>().OrderBy(c => c).ToList(),
            GenderRequirements = Enum.GetValues<GenderRequirement>().OrderBy(g => g).ToList(),
            MinimumDegreeLevels = Enum.GetValues<DegreeLevel>().OrderBy(d => d).ToList(),
        });
    }

    public async Task<OperationResult<PublicJobPostingDetailDto>> GetByIdAsync(Guid jobPostingId)
    {
        var posting = await context.JobPostingRequests
            .AsNoTracking()
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .ThenInclude(l => l.Province)
            .Include(j => j.SalaryRange)
            .Include(j => j.ContractTypes)
            .Include(j => j.EssentialSkills)
            .ThenInclude(s => s.Skill)
            .Include(j => j.PersonalTraits)
            .Include(j => j.Benefits)
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.JobPostingId == jobPostingId && j.Status == JobPostingStatus.Active);

        if (posting == null)
            return OperationResult<PublicJobPostingDetailDto>.Failure("آگهی یافت نشد");

        var orgSummary = await GetOrganizationSummaryAsync(posting.UserId, posting.User);

        return OperationResult<PublicJobPostingDetailDto>.Success(new PublicJobPostingDetailDto
        {
            JobPostingId = posting.JobPostingId,
            JobCategoryName = posting.JobCategory?.CategoryName ?? string.Empty,
            LocationName = posting.Location?.LocationName ?? string.Empty,
            ProvinceName = posting.Location?.Province?.ProvinceName ?? string.Empty,
            SalaryRangeName = posting.SalaryRange?.SalaryRangeDescription ?? string.Empty,
            MinimumWorkExperienceYears = posting.MinimumWorkExperienceYears,
            JobDescription = posting.JobDescription,
            ContractTypes = posting.ContractTypes.Select(c => c.ContractType).ToList(),
            RequiredPersonalTraits = posting.PersonalTraits.OrderBy(t => t.SortOrder).Select(t => t.TraitText).ToList(),
            EssentialSkillNames = posting.EssentialSkills.Select(s => s.Skill?.SkillName ?? string.Empty).ToList(),
            Benefits = posting.Benefits.OrderBy(b => b.SortOrder).Select(b => b.BenefitText).ToList(),
            GenderRequirement = posting.GenderRequirement,
            MilitaryServiceRequirement = posting.MilitaryServiceRequirement,
            MinimumDegreeLevel = posting.MinimumDegreeLevel,
            AdditionalNotes = posting.AdditionalNotes,
            PublishedAt = posting.PublishedAt,
            OrganizationSummary = orgSummary,
        });
    }

    private static string BuildTitle(JobPostingRequest posting)
    {
        var category = posting.JobCategory?.CategoryName ?? string.Empty;
        var location = posting.Location?.LocationName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(category))
            return location;
        if (string.IsNullOrWhiteSpace(location))
            return category;
        return $"{category} - {location}";
    }

    private async Task<JobPostingOrgSummaryDto?> GetOrganizationSummaryAsync(Guid userId, User? user = null)
    {
        user ??= await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return null;

        CenterProfile? profile = null;
        if (user.CenterProfileId.HasValue)
            profile = await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);
        else if (user.UserType == UserType.Store)
            profile = await centerProfileRepository.GetByOwnerUserIdAsync(userId);

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
}
