using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ActivityLog;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ActivityLogService(
    LabConnectDbContext context,
    ICenterProfileRepository centerProfileRepository) : IActivityLogService
{
    public async Task<OperationResult<PagedResult<ActivityLogRecordGroupDto>>> GetGroupedAsync(
        Guid requesterUserId,
        GetActivityLogsQuery query)
    {
        var access = await ResolveAccessAsync(requesterUserId);
        if (access.Error != null)
            return OperationResult<PagedResult<ActivityLogRecordGroupDto>>.Failure(access.Error);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var baseQuery = ApplyScope(context.ActivityLogs.AsNoTracking(), access);
        baseQuery = ApplyFilters(baseQuery, query);
        // Skip rows that never captured a displayable value (empty shells).
        baseQuery = baseQuery.Where(l => l.OldValue != null || l.NewValue != null);

        var recordGroupsQuery = baseQuery
            .GroupBy(l => new { l.EntityName, l.RecordKey })
            .Select(g => new
            {
                g.Key.EntityName,
                g.Key.RecordKey,
                LatestAt = g.Max(x => x.CreatedAt),
            })
            .OrderByDescending(x => x.LatestAt);

        var totalCount = await recordGroupsQuery.CountAsync();
        var pageKeys = await recordGroupsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (pageKeys.Count == 0)
        {
            return OperationResult<PagedResult<ActivityLogRecordGroupDto>>.Success(new PagedResult<ActivityLogRecordGroupDto>
            {
                Items = [],
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            });
        }

        var entityNames = pageKeys.Select(k => k.EntityName).Distinct().ToList();
        var recordKeys = pageKeys.Select(k => k.RecordKey).Distinct().ToList();

        var rows = await ApplyScope(context.ActivityLogs.AsNoTracking(), access)
            .Where(l => entityNames.Contains(l.EntityName) && recordKeys.Contains(l.RecordKey))
            .Where(l => l.OldValue != null || l.NewValue != null)
            .OrderBy(l => l.CreatedAt)
            .ThenBy(l => l.Id)
            .ToListAsync();

        // Keep only rows that match exact (EntityName, RecordKey) pairs on this page
        var keySet = pageKeys.Select(k => $"{k.EntityName}\u001f{k.RecordKey}").ToHashSet();
        rows = rows.Where(r => keySet.Contains($"{r.EntityName}\u001f{r.RecordKey}")).ToList();

        var batches = await MapBatchesAsync(rows);
        await FillMissingRecordTitlesAsync(batches);

        var groups = pageKeys.Select(key =>
        {
            var recordBatches = batches
                .Where(b => b.EntityName == key.EntityName && b.RecordKey == key.RecordKey)
                .OrderBy(b => b.CreatedAt)
                .ThenBy(b => b.BatchId)
                .ToList();
            var latest = recordBatches.LastOrDefault();
            return new ActivityLogRecordGroupDto
            {
                EntityName = key.EntityName,
                RecordKey = key.RecordKey,
                RecordTitle = latest?.RecordTitle,
                CenterProfileId = latest?.CenterProfileId,
                CenterName = latest?.CenterName,
                LatestAt = key.LatestAt,
                BatchCount = recordBatches.Count,
                Batches = recordBatches,
            };
        })
        .Where(g => g.BatchCount > 0)
        .ToList();

        return OperationResult<PagedResult<ActivityLogRecordGroupDto>>.Success(new PagedResult<ActivityLogRecordGroupDto>
        {
            Items = groups,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<PagedResult<ActivityLogBatchDto>>> GetBatchesAsync(
        Guid requesterUserId,
        GetActivityLogsQuery query)
    {
        var access = await ResolveAccessAsync(requesterUserId);
        if (access.Error != null)
            return OperationResult<PagedResult<ActivityLogBatchDto>>.Failure(access.Error);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var baseQuery = ApplyScope(context.ActivityLogs.AsNoTracking(), access);
        baseQuery = ApplyFilters(baseQuery, query);
        // Skip rows that never captured a displayable value (empty shells).
        baseQuery = baseQuery.Where(l => l.OldValue != null || l.NewValue != null);

        var batchIdsQuery = baseQuery
            .GroupBy(l => l.BatchId)
            .Select(g => new { BatchId = g.Key, CreatedAt = g.Max(x => x.CreatedAt) })
            .OrderBy(x => x.CreatedAt);

        var totalCount = await batchIdsQuery.CountAsync();
        var pageBatches = await batchIdsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.BatchId)
            .ToListAsync();

        var rows = await ApplyFilters(
                ApplyScope(context.ActivityLogs.AsNoTracking(), access)
                    .Where(l => pageBatches.Contains(l.BatchId))
                    .Where(l => l.OldValue != null || l.NewValue != null),
                query)
            .OrderBy(l => l.CreatedAt)
            .ThenBy(l => l.Id)
            .ToListAsync();

        var batches = await MapBatchesAsync(rows);
        // Preserve page order (a BatchId may map to one DTO after per-record grouping)
        var ordered = pageBatches
            .SelectMany(id => batches.Where(b => b.BatchId == id))
            .ToList();

        return OperationResult<PagedResult<ActivityLogBatchDto>>.Success(new PagedResult<ActivityLogBatchDto>
        {
            Items = ordered,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<PagedResult<ActivityLogBatchDto>>> GetByRecordAsync(
        Guid requesterUserId,
        GetActivityLogsByRecordQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.EntityName) || string.IsNullOrWhiteSpace(query.RecordKey))
            return OperationResult<PagedResult<ActivityLogBatchDto>>.Failure("موجودیت و شناسه رکورد الزامی است");

        return await GetBatchesAsync(requesterUserId, new GetActivityLogsQuery
        {
            EntityName = query.EntityName.Trim(),
            RecordKey = query.RecordKey.Trim(),
            Page = query.Page,
            PageSize = query.PageSize,
        });
    }

    public async Task<OperationResult<ActivityLogBatchDto>> GetBatchAsync(Guid requesterUserId, Guid batchId)
    {
        var access = await ResolveAccessAsync(requesterUserId);
        if (access.Error != null)
            return OperationResult<ActivityLogBatchDto>.Failure(access.Error);

        var rows = await ApplyScope(context.ActivityLogs.AsNoTracking(), access)
            .Where(l => l.BatchId == batchId)
            .OrderBy(l => l.Id)
            .ToListAsync();

        if (rows.Count == 0)
            return OperationResult<ActivityLogBatchDto>.Failure("لاگ یافت نشد");

        var batches = await MapBatchesAsync(rows);
        return OperationResult<ActivityLogBatchDto>.Success(batches[0]);
    }

    public async Task<OperationResult<List<ActivityLogUserOptionDto>>> GetCenterUsersAsync(Guid requesterUserId)
    {
        var access = await ResolveAccessAsync(requesterUserId);
        if (access.Error != null)
            return OperationResult<List<ActivityLogUserOptionDto>>.Failure(access.Error);

        if (access.IsSiteAdmin || !access.CenterProfileId.HasValue)
            return OperationResult<List<ActivityLogUserOptionDto>>.Success([]);

        var users = await context.Users.AsNoTracking()
            .Where(u => u.CenterProfileId == access.CenterProfileId)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Username,
                u.MobileNumber,
            })
            .ToListAsync();

        var options = users.Select(u =>
        {
            var name = $"{u.FirstName} {u.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name))
                name = u.Username;
            return new ActivityLogUserOptionDto
            {
                Id = u.Id,
                DisplayName = name,
                MobileNumber = u.MobileNumber,
            };
        }).ToList();

        return OperationResult<List<ActivityLogUserOptionDto>>.Success(options);
    }

    private static IQueryable<Domain.Entities.ActivityLog> ApplyFilters(
        IQueryable<Domain.Entities.ActivityLog> query,
        GetActivityLogsQuery filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.EntityName))
            query = query.Where(l => l.EntityName == filters.EntityName.Trim());
        if (!string.IsNullOrWhiteSpace(filters.RecordKey))
            query = query.Where(l => l.RecordKey == filters.RecordKey.Trim());
        if (filters.CenterProfileId.HasValue)
            query = query.Where(l => l.CenterProfileId == filters.CenterProfileId.Value);
        if (filters.UserId.HasValue)
            query = query.Where(l => l.UserId == filters.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filters.Action))
            query = query.Where(l => l.Action == filters.Action.Trim());
        if (filters.From.HasValue)
            query = query.Where(l => l.CreatedAt >= filters.From.Value);
        if (filters.To.HasValue)
            query = query.Where(l => l.CreatedAt <= filters.To.Value);
        return query;
    }

    private static IQueryable<Domain.Entities.ActivityLog> ApplyScope(
        IQueryable<Domain.Entities.ActivityLog> query,
        AccessContext access)
    {
        if (access.IsSiteAdmin)
            return query;

        return query.Where(l => l.CenterProfileId == access.CenterProfileId);
    }

    private async Task<List<ActivityLogBatchDto>> MapBatchesAsync(List<Domain.Entities.ActivityLog> rows)
    {
        var userIds = rows.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var centerIds = rows.Where(r => r.CenterProfileId.HasValue).Select(r => r.CenterProfileId!.Value).Distinct().ToList();

        var users = await context.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Username })
            .ToListAsync();
        var userMap = users.ToDictionary(
            u => u.Id,
            u =>
            {
                var fullName = $"{u.FirstName} {u.LastName}".Trim();
                return string.IsNullOrWhiteSpace(fullName) ? u.Username : fullName;
            });

        var centers = await context.CenterProfiles.AsNoTracking()
            .Where(c => centerIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
        var centerMap = centers.ToDictionary(c => c.Id, c => c.Name);

        // Batches are save-events for a single record keyed by (EntityName, RecordKey).
        // Include BatchId so multiple edits of the same record stay separate in the timeline.
        var batches = rows
            .GroupBy(r => new { r.EntityName, r.RecordKey, r.BatchId })
            .Select(g =>
            {
                var first = g.OrderBy(x => x.Id).First();
                var title = first.RecordTitle
                    ?? g.Select(x => x.RecordTitle).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                var changes = g.OrderBy(x => x.Id)
                    .Where(x => !string.IsNullOrEmpty(x.OldValue) || !string.IsNullOrEmpty(x.NewValue))
                    .Select(x => new ActivityLogChangeDto
                    {
                        Id = x.Id,
                        FieldName = x.FieldName,
                        OldValue = x.OldValue,
                        NewValue = x.NewValue,
                    })
                    .ToList();
                return new ActivityLogBatchDto
                {
                    BatchId = g.Key.BatchId,
                    UserId = first.UserId,
                    UserDisplayName = first.UserId.HasValue && userMap.TryGetValue(first.UserId.Value, out var un) ? un : null,
                    UserType = first.UserType,
                    Action = first.Action,
                    EntityName = g.Key.EntityName,
                    RecordKey = g.Key.RecordKey,
                    RecordTitle = title,
                    CenterProfileId = first.CenterProfileId,
                    CenterName = first.CenterProfileId.HasValue && centerMap.TryGetValue(first.CenterProfileId.Value, out var cn) ? cn : null,
                    IpAddress = first.IpAddress,
                    CreatedAt = first.CreatedAt,
                    Changes = changes,
                };
            })
            .Where(b => b.Changes.Count > 0)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.EntityName)
            .ThenBy(b => b.RecordKey)
            .ThenBy(b => b.BatchId)
            .ToList();

        await FillMissingRecordTitlesAsync(batches);
        return batches;
    }

    private async Task FillMissingRecordTitlesAsync(List<ActivityLogBatchDto> batches)
    {
        var missing = batches
            .Where(b => string.IsNullOrWhiteSpace(b.RecordTitle) && !string.IsNullOrWhiteSpace(b.RecordKey))
            .GroupBy(b => b.EntityName)
            .ToList();

        foreach (var group in missing)
        {
            var keys = group.Select(b => b.RecordKey).Distinct().ToList();
            var titles = await ResolveTitlesAsync(group.Key, keys);

            foreach (var batch in group)
            {
                if (titles.TryGetValue(batch.RecordKey, out var title) && !string.IsNullOrWhiteSpace(title))
                    batch.RecordTitle = title;
            }
        }
    }

    private async Task<Dictionary<string, string>> ResolveTitlesAsync(string entityName, List<string> keys)
    {
        switch (entityName)
        {
            case nameof(Brand):
            {
                var ids = ParseInts(keys);
                return await context.Brands.AsNoTracking()
                    .Where(b => ids.Contains(b.BrandId))
                    .ToDictionaryAsync(b => b.BrandId.ToString(), b => b.Title);
            }
            case nameof(ProductCategory):
            {
                var ids = ParseInts(keys);
                return await context.ProductCategories.AsNoTracking()
                    .Where(c => ids.Contains(c.ProductCategoryId))
                    .ToDictionaryAsync(c => c.ProductCategoryId.ToString(), c => c.Title);
            }
            case nameof(ProductCategoryGroup):
            {
                var ids = ParseInts(keys);
                return await context.ProductCategoryGroups.AsNoTracking()
                    .Where(g => ids.Contains(g.ProductCategoryGroupId))
                    .ToDictionaryAsync(g => g.ProductCategoryGroupId.ToString(), g => g.Name);
            }
            case nameof(ProductAttribute):
            {
                var ids = ParseInts(keys);
                return await context.ProductAttributes.AsNoTracking()
                    .Where(a => ids.Contains(a.ProductAttributeId))
                    .ToDictionaryAsync(a => a.ProductAttributeId.ToString(), a => a.Title);
            }
            case nameof(SpecialOffer):
            {
                var ids = ParseLongs(keys);
                return await context.SpecialOffers.AsNoTracking()
                    .Where(o => ids.Contains(o.Id))
                    .ToDictionaryAsync(o => o.Id.ToString(), o => o.Title);
            }
            case nameof(Product):
            {
                var ids = ParseGuids(keys);
                return await context.Products.AsNoTracking()
                    .Where(p => ids.Contains(p.ProductId))
                    .ToDictionaryAsync(p => p.ProductId.ToString(), p => p.Title);
            }
            case nameof(CenterProfile):
            {
                var ids = ParseGuids(keys);
                return await context.CenterProfiles.AsNoTracking()
                    .Where(c => ids.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id.ToString(), c => c.Name);
            }
            case nameof(DeviceGroup):
            {
                var ids = ParseLongs(keys);
                return await context.DeviceGroups.AsNoTracking()
                    .Where(d => ids.Contains(d.Id))
                    .ToDictionaryAsync(d => d.Id.ToString(), d => d.Title);
            }
            case nameof(KitGroup):
            {
                var ids = ParseLongs(keys);
                return await context.KitGroups.AsNoTracking()
                    .Where(k => ids.Contains(k.Id))
                    .ToDictionaryAsync(k => k.Id.ToString(), k => k.Title);
            }
            case nameof(LabAgreement):
            {
                var ids = ParseLongs(keys);
                return await context.LabAgreements.AsNoTracking()
                    .Where(a => ids.Contains(a.Id))
                    .ToDictionaryAsync(a => a.Id.ToString(), a => a.Title);
            }
            case nameof(ContentGroup):
            {
                var ids = ParseInts(keys);
                return await context.ContentGroups.AsNoTracking()
                    .Where(c => ids.Contains(c.ContentGroupId))
                    .ToDictionaryAsync(c => c.ContentGroupId.ToString(), c => c.Title);
            }
            case nameof(ContentPost):
            {
                var ids = ParseGuids(keys);
                return await context.ContentPosts.AsNoTracking()
                    .Where(c => ids.Contains(c.ContentPostId))
                    .ToDictionaryAsync(c => c.ContentPostId.ToString(), c => c.Title);
            }
            case nameof(SliderGroup):
            {
                var ids = ParseGuids(keys);
                return await context.SliderGroups.AsNoTracking()
                    .Where(s => ids.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id.ToString(), s => s.Title);
            }
            case nameof(SliderSlide):
            {
                var ids = ParseGuids(keys);
                return await context.SliderSlides.AsNoTracking()
                    .Where(s => ids.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id.ToString(), s => s.Title ?? s.Id.ToString());
            }
            case nameof(SiteUsefulLink):
            {
                var ids = ParseGuids(keys);
                return await context.SiteUsefulLinks.AsNoTracking()
                    .Where(s => ids.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id.ToString(), s => s.Title);
            }
            case nameof(SiteSettings):
            {
                var ids = ParseInts(keys);
                return await context.SiteSettings.AsNoTracking()
                    .Where(s => ids.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id.ToString(), s => s.SiteTitle);
            }
            case nameof(TestInfo):
            {
                var ids = ParseLongs(keys);
                return await context.TestInfos.AsNoTracking()
                    .Where(t => ids.Contains(t.Id))
                    .ToDictionaryAsync(t => t.Id.ToString(), t => t.FullName ?? t.ShortName ?? t.Id.ToString());
            }
            case nameof(SiteUserPermission):
            {
                var ids = ParseGuids(keys);
                var rows = await context.SiteUserPermissions.AsNoTracking()
                    .Where(p => ids.Contains(p.Id))
                    .Select(p => new
                    {
                        p.Id,
                        p.User.FirstName,
                        p.User.LastName,
                        p.User.Username,
                        p.User.MobileNumber,
                    })
                    .ToListAsync();
                return rows.ToDictionary(
                    x => x.Id.ToString(),
                    x =>
                    {
                        var name = $"{x.FirstName} {x.LastName}".Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                            return name;
                        if (!string.IsNullOrWhiteSpace(x.Username))
                            return x.Username!;
                        return string.IsNullOrWhiteSpace(x.MobileNumber) ? x.Id.ToString() : x.MobileNumber;
                    });
            }
            case nameof(User):
            {
                var ids = ParseGuids(keys);
                return await context.Users.AsNoTracking()
                    .Where(u => ids.Contains(u.Id))
                    .ToDictionaryAsync(
                        u => u.Id.ToString(),
                        u =>
                        {
                            var name = $"{u.FirstName} {u.LastName}".Trim();
                            return string.IsNullOrWhiteSpace(name) ? u.Username : name;
                        });
            }
            case nameof(OrganizationLocation):
            {
                var ids = ParseGuids(keys);
                return await context.OrganizationLocations.AsNoTracking()
                    .Where(o => ids.Contains(o.LocationId))
                    .ToDictionaryAsync(o => o.LocationId.ToString(), o => o.LocationName);
            }
            case nameof(ProductExpertReview):
            {
                var ids = ParseGuids(keys);
                return await context.ProductExpertReviews.AsNoTracking()
                    .Where(r => ids.Contains(r.Id))
                    .ToDictionaryAsync(r => r.Id.ToString(), r => r.Title);
            }
            case nameof(LabAgreementAttachment):
            {
                var ids = ParseLongs(keys);
                return await context.LabAgreementAttachments.AsNoTracking()
                    .Where(a => ids.Contains(a.Id))
                    .ToDictionaryAsync(a => a.Id.ToString(), a => a.FileName);
            }
            case nameof(LabAgreementTestPrice):
            {
                var ids = ParseLongs(keys);
                return await context.LabAgreementTestPrices.AsNoTracking()
                    .Where(a => ids.Contains(a.Id))
                    .ToDictionaryAsync(a => a.Id.ToString(), a => a.TestName);
            }
            case nameof(Skill):
            {
                var ids = ParseInts(keys);
                return await context.Skills.AsNoTracking()
                    .Where(s => ids.Contains(s.SkillId))
                    .ToDictionaryAsync(s => s.SkillId.ToString(), s => s.SkillName);
            }
            default:
                return new Dictionary<string, string>();
        }
    }

    private static List<int> ParseInts(List<string> keys)
        => keys.Select(k => int.TryParse(k, out var id) ? id : (int?)null)
            .Where(id => id.HasValue).Select(id => id!.Value).ToList();

    private static List<long> ParseLongs(List<string> keys)
        => keys.Select(k => long.TryParse(k, out var id) ? id : (long?)null)
            .Where(id => id.HasValue).Select(id => id!.Value).ToList();

    private static List<Guid> ParseGuids(List<string> keys)
        => keys.Select(k => Guid.TryParse(k, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue).Select(id => id!.Value).ToList();

    private async Task<AccessContext> ResolveAccessAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking()
            .Include(u => u.CenterProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return new AccessContext { Error = "کاربر یافت نشد" };

        if (user.UserType is UserType.Administrator or UserType.Admin)
            return new AccessContext { IsSiteAdmin = true };

        if (user.UserType is not (UserType.Store or UserType.AdminLab or UserType.UserLab))
            return new AccessContext { Error = "دسترسی مجاز نیست" };

        var profile = await ResolveCenterProfileAsync(user);
        if (profile is null)
            return new AccessContext { Error = "پروفایل مرکز یافت نشد" };

        return new AccessContext { CenterProfileId = profile.Id };
    }

    private async Task<CenterProfile?> ResolveCenterProfileAsync(User user)
    {
        if (user.CenterProfileId.HasValue)
            return await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);

        if (user.UserType == UserType.AdminLab)
        {
            var labCode = user.CenterProfile?.LabCode;
            if (labCode.HasValue)
                return await centerProfileRepository.GetByLabCodeAsync(labCode.Value);
        }

        if (user.UserType == UserType.Store)
            return await centerProfileRepository.GetByOwnerUserIdAsync(user.Id);

        return null;
    }

    private sealed class AccessContext
    {
        public bool IsSiteAdmin { get; init; }
        public Guid? CenterProfileId { get; init; }
        public string? Error { get; init; }
    }
}
