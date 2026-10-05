using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class CenterProfileRepository(LabConnectDbContext context)
    : LabConnectRepository<CenterProfile>(context), ICenterProfileRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<CenterProfile?> GetByLabCodeAsync(int labCode)
        => _context.CenterProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CenterType == CenterType.Lab && p.LabCode == labCode);

    public Task<CenterProfile?> GetByLabCodeNewAsync(int labCodeNew)
        => _context.CenterProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CenterType == CenterType.Lab && p.LabCodeNew == labCodeNew);

    public Task<CenterProfile?> GetByOwnerUserIdAsync(Guid ownerUserId)
        => _context.CenterProfiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.CenterType == CenterType.Store && p.OwnerUserId == ownerUserId);

    public Task<CenterProfile?> GetByIdWithTrackingAsync(Guid id)
        => _context.CenterProfiles.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<PagedResult<CenterProfileListItemDto>> GetLaboratoriesPagedAsync(GetCenterProfilesQuery query)
    {
        var q = from profile in _context.CenterProfiles.AsNoTracking()
                where profile.CenterType == CenterType.Lab
                join admin in _context.Users.AsNoTracking().Where(u => u.UserType == UserType.AdminLab)
                    on profile.Id equals admin.CenterProfileId into adminGroup
                from admin in adminGroup.DefaultIfEmpty()
                select new { profile, admin };

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            q = q.Where(x => x.profile.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.LabCode) && int.TryParse(query.LabCode.Trim(), out var labCode))
            q = q.Where(x => x.profile.LabCode == labCode);

        if (!string.IsNullOrWhiteSpace(query.Mobile))
        {
            var mobile = query.Mobile.Trim();
            q = q.Where(x => x.admin != null && x.admin.MobileNumber.Contains(mobile));
        }

        if (query.CompletionStatus == ProfileCompletionFilter.Complete)
            q = q.Where(x => x.profile.IsComplete);
        else if (query.CompletionStatus == ProfileCompletionFilter.Incomplete)
            q = q.Where(x => !x.profile.IsComplete);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(x => x.profile.CompletedAt ?? DateTime.MinValue)
            .ThenByDescending(x => x.profile.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CenterProfileListItemDto
            {
                Id = x.profile.Id,
                Name = x.profile.Name,
                LabCode = x.profile.LabCode,
                MobileNumber = x.admin != null ? x.admin.MobileNumber : string.Empty,
                Status = x.profile.Status,
                IsComplete = x.profile.IsComplete,
                IsApproved = x.profile.IsApproved,
                IsApiKeyEnabled = x.profile.IsApiKeyEnabled,
            })
            .ToListAsync();

        return new PagedResult<CenterProfileListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<CenterProfileListItemDto>> GetShopsPagedAsync(GetCenterProfilesQuery query)
    {
        var q = from profile in _context.CenterProfiles.AsNoTracking()
                where profile.CenterType == CenterType.Store
                join owner in _context.Users.AsNoTracking()
                    on profile.OwnerUserId equals owner.Id
                select new { profile, owner };

        if (!string.IsNullOrWhiteSpace(query.Name))
        {
            var name = query.Name.Trim();
            q = q.Where(x => x.profile.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(query.Mobile))
        {
            var mobile = query.Mobile.Trim();
            q = q.Where(x => x.owner.MobileNumber.Contains(mobile));
        }

        if (query.CompletionStatus == ProfileCompletionFilter.Complete)
            q = q.Where(x => x.profile.IsComplete);
        else if (query.CompletionStatus == ProfileCompletionFilter.Incomplete)
            q = q.Where(x => !x.profile.IsComplete);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(x => x.profile.CompletedAt ?? DateTime.MinValue)
            .ThenByDescending(x => x.profile.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new CenterProfileListItemDto
            {
                Id = x.profile.Id,
                Name = x.profile.Name,
                MobileNumber = x.owner.MobileNumber,
                Status = x.profile.Status,
                IsComplete = x.profile.IsComplete,
                IsApproved = x.profile.IsApproved,
                IsApiKeyEnabled = x.profile.IsApiKeyEnabled,
            })
            .ToListAsync();

        return new PagedResult<CenterProfileListItemDto>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public Task<string?> GetAdminLabMobileAsync(int labCode)
        => _context.Users.AsNoTracking()
            .Where(u => u.UserType == UserType.AdminLab
                && u.CenterProfile != null
                && (u.CenterProfile.LabCode == labCode || u.CenterProfile.LabCodeNew == labCode))
            .Select(u => u.MobileNumber)
            .FirstOrDefaultAsync();

    public Task<Guid?> GetAdminLabUserIdAsync(int labCode)
        => _context.Users.AsNoTracking()
            .Where(u => u.UserType == UserType.AdminLab
                && u.CenterProfile != null
                && (u.CenterProfile.LabCode == labCode || u.CenterProfile.LabCodeNew == labCode))
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync();

    public async Task<CenterProfile?> GetProfileByUserId(Guid userId) => await _context.Users.Where(x => x.Id == userId).Select(x => x.CenterProfile).FirstOrDefaultAsync();

}
