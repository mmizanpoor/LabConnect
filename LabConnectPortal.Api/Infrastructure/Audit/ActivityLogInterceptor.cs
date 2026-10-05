using System.Collections.Concurrent;
using System.Security.Claims;
using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LabConnectPortal.Api.Infrastructure.Audit;

/// <summary>
/// Field-level audit: known-key changes join the current SaveChanges;
/// identity-key Creates are deferred until after the PK is generated.
/// </summary>
public sealed class ActivityLogInterceptor(
    IAuditableEntityRegistry registry,
    IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<DbContext, List<PendingActivityChange>> Deferred = new();
    private bool _suppress;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistDeferred(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PersistDeferredAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
            Deferred.TryRemove(eventData.Context, out _);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Deferred.TryRemove(eventData.Context, out _);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void Capture(DbContext? context)
    {
        if (_suppress || context is null)
            return;

        var pending = BuildPendingChanges(context);
        if (pending.Count == 0)
            return;

        var ready = pending.Where(p => !p.NeedsRecordKey).ToList();
        var deferred = pending.Where(p => p.NeedsRecordKey).ToList();

        if (ready.Count > 0)
        {
            var logs = MaterializeLogs(ready);
            if (logs.Count > 0)
                context.Set<ActivityLog>().AddRange(logs);
        }

        if (deferred.Count > 0)
            Deferred[context] = deferred;
        else
            Deferred.TryRemove(context, out _);
    }

    private void PersistDeferred(DbContext? context)
    {
        if (_suppress || context is null)
            return;
        if (!Deferred.TryRemove(context, out var deferred) || deferred.Count == 0)
            return;

        ResolveDeferredKeys(deferred);
        var logs = MaterializeLogs(deferred.Where(p => !string.IsNullOrWhiteSpace(p.RecordKey)).ToList());
        if (logs.Count == 0)
            return;

        _suppress = true;
        try
        {
            context.Set<ActivityLog>().AddRange(logs);
            context.SaveChanges();
        }
        finally
        {
            _suppress = false;
        }
    }

    private async Task PersistDeferredAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (_suppress || context is null)
            return;
        if (!Deferred.TryRemove(context, out var deferred) || deferred.Count == 0)
            return;

        ResolveDeferredKeys(deferred);
        var logs = MaterializeLogs(deferred.Where(p => !string.IsNullOrWhiteSpace(p.RecordKey)).ToList());
        if (logs.Count == 0)
            return;

        _suppress = true;
        try
        {
            context.Set<ActivityLog>().AddRange(logs);
            await context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _suppress = false;
        }
    }

    private void ResolveDeferredKeys(List<PendingActivityChange> deferred)
    {
        foreach (var change in deferred)
        {
            if (!registry.TryGetDescriptor(change.ClrType, out var descriptor))
                continue;
            var key = descriptor.GetRecordKey(change.Entity);
            if (string.IsNullOrWhiteSpace(key) || key is "0" or "00000000-0000-0000-0000-000000000000")
                continue;
            change.RecordKey = key;

            if (string.IsNullOrWhiteSpace(change.RecordTitle))
                change.RecordTitle = ResolveRecordTitleFromEntity(change.Entity, descriptor);
        }
    }

    private static string? ResolveRecordTitleFromEntity(object entity, AuditableEntityDescriptor descriptor)
    {
        if (descriptor.GetRecordTitle is not null)
        {
            var custom = TruncateTitle(descriptor.GetRecordTitle(entity));
            if (!string.IsNullOrWhiteSpace(custom))
                return custom;
        }

        foreach (var name in TitlePropertyNames)
        {
            var prop = entity.GetType().GetProperty(name);
            if (prop is null)
                continue;
            var text = TruncateTitle(prop.GetValue(entity)?.ToString());
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        if (entity is User user)
        {
            var name = TruncateTitle($"{user.FirstName} {user.LastName}".Trim());
            if (!string.IsNullOrWhiteSpace(name))
                return name;
            return TruncateTitle(user.Username);
        }

        return null;
    }

    private static readonly string[] TitlePropertyNames =
    [
        "Title",
        "Name",
        "LocationName",
        "Username",
        "FullName",
        "SiteTitle",
        "FileName",
        "TestName",
        "JobTitle",
        "ContractNumber",
        "SkillName",
        "BenefitText",
        "TraitText",
        "CompanyName",
        "InstitutionName",
    ];

    private List<PendingActivityChange> BuildPendingChanges(DbContext context)
    {
        var list = new List<PendingActivityChange>();
        var actorCenterProfileId = ResolveActorCenterProfileId(context);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is ActivityLog)
                continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var clrType = entry.Metadata.ClrType;
            if (!registry.TryGetDescriptor(clrType, out var descriptor))
                continue;

            var action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Deleted => "Delete",
                _ => "Update",
            };

            var centerProfileId = actorCenterProfileId ?? descriptor.GetCenterProfileId(entry, context);
            var (recordKey, needsKey) = ResolveRecordKey(entry, descriptor);
            var recordTitle = ResolveRecordTitle(entry, descriptor);

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey())
                    continue;
                if (property.Metadata.IsShadowProperty())
                    continue;
                if (registry.IsIgnoredProperty(descriptor, property.Metadata.Name))
                    continue;

                if (entry.State == EntityState.Modified && !property.IsModified)
                    continue;

                if (entry.State == EntityState.Added)
                {
                    var newValue = ActivityValueFormatter.Format(property.CurrentValue);
                    if (newValue is null)
                        continue;

                    list.Add(CreateChange(entry.Entity, descriptor, action, property.Metadata.Name, null, newValue, centerProfileId, recordKey, recordTitle, needsKey));
                    continue;
                }

                if (entry.State == EntityState.Deleted)
                {
                    var deletedOld = ActivityValueFormatter.Format(property.OriginalValue);
                    if (deletedOld is null)
                        continue;

                    list.Add(CreateChange(
                        entry.Entity,
                        descriptor,
                        action,
                        property.Metadata.Name,
                        deletedOld,
                        null,
                        centerProfileId,
                        recordKey,
                        recordTitle,
                        needsKey: false));
                    continue;
                }

                var oldDisplay = ActivityValueFormatter.Format(property.OriginalValue);
                var newDisplay = ActivityValueFormatter.Format(property.CurrentValue);
                if (string.Equals(oldDisplay, newDisplay, StringComparison.Ordinal))
                    continue;

                list.Add(CreateChange(entry.Entity, descriptor, action, property.Metadata.Name, oldDisplay, newDisplay, centerProfileId, recordKey, recordTitle, needsKey: false));
            }
        }

        return list;
    }

    private static PendingActivityChange CreateChange(
        object entity,
        AuditableEntityDescriptor descriptor,
        string action,
        string fieldName,
        string? oldValue,
        string? newValue,
        Guid? centerProfileId,
        string? recordKey,
        string? recordTitle,
        bool needsKey)
        => new()
        {
            Entity = entity,
            ClrType = descriptor.ClrType,
            EntityName = descriptor.EntityName,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            CenterProfileId = centerProfileId,
            RecordKey = recordKey,
            RecordTitle = recordTitle,
            NeedsRecordKey = needsKey,
        };

    private static string? ResolveRecordTitle(EntityEntry entry, AuditableEntityDescriptor descriptor)
    {
        if (descriptor.GetRecordTitle is not null)
        {
            var custom = TruncateTitle(descriptor.GetRecordTitle(entry.Entity));
            if (!string.IsNullOrWhiteSpace(custom))
                return custom;
        }

        foreach (var name in TitlePropertyNames)
        {
            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == name);
            if (prop is null)
                continue;

            var value = entry.State == EntityState.Deleted
                ? prop.OriginalValue
                : prop.CurrentValue ?? prop.OriginalValue;

            var text = TruncateTitle(value?.ToString());
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        return null;
    }

    private static string? TruncateTitle(string? value)
    {
        var text = value?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return null;
        return text.Length > 300 ? text[..300] : text;
    }

    private static (string? RecordKey, bool NeedsKey) ResolveRecordKey(EntityEntry entry, AuditableEntityDescriptor descriptor)
    {
        foreach (var keyProp in entry.Properties.Where(p => p.Metadata.IsPrimaryKey()))
        {
            if (keyProp.IsTemporary)
                return (null, true);

            var value = entry.State == EntityState.Deleted
                ? keyProp.OriginalValue
                : keyProp.CurrentValue ?? keyProp.OriginalValue;

            if (value is null)
                continue;

            var text = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(text) || text is "0" or "00000000-0000-0000-0000-000000000000")
                return (null, entry.State == EntityState.Added);

            return (text, false);
        }

        var fallback = descriptor.GetRecordKey(entry.Entity);
        if (string.IsNullOrWhiteSpace(fallback) || fallback is "0" or "00000000-0000-0000-0000-000000000000")
            return (null, entry.State == EntityState.Added);

        return (fallback, false);
    }

    private Guid? ResolveActorCenterProfileId(DbContext context)
    {
        var http = httpContextAccessor.HttpContext;
        var user = http?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId))
            return null;

        var tracked = context.Set<User>().Local.FirstOrDefault(u => u.Id == userId);
        return tracked?.CenterProfileId;
    }

    private List<ActivityLog> MaterializeLogs(List<PendingActivityChange> pending)
    {
        var http = httpContextAccessor.HttpContext;
        var user = http?.User;
        Guid? userId = null;
        var userType = "Anonymous";
        if (user?.Identity?.IsAuthenticated == true)
        {
            var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(idClaim, out var parsed))
                userId = parsed;
            userType = user.FindFirst("userType")?.Value ?? "Unknown";
        }

        var ip = http?.Connection.RemoteIpAddress?.ToString();
        var userAgent = http?.Request.Headers.UserAgent.ToString();
        if (userAgent is { Length: > 500 })
            userAgent = userAgent[..500];

        var now = DateTime.UtcNow.ToLocalTime();
        var logs = new List<ActivityLog>(pending.Count);

        // One BatchId per (EntityName, RecordKey) within this SaveChanges.
        foreach (var recordGroup in pending
                     .Where(p => !string.IsNullOrWhiteSpace(p.RecordKey))
                     .GroupBy(p => (p.EntityName, RecordKey: p.RecordKey!)))
        {
            var batchId = Guid.NewGuid();
            foreach (var change in recordGroup)
            {
                logs.Add(new ActivityLog
                {
                    BatchId = batchId,
                    UserId = userId,
                    UserType = userType,
                    Action = change.Action,
                    EntityName = change.EntityName,
                    RecordKey = change.RecordKey!,
                    RecordTitle = change.RecordTitle,
                    FieldName = change.FieldName,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                    CenterProfileId = change.CenterProfileId,
                    IpAddress = ip,
                    UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent,
                    CreatedAt = now,
                });
            }
        }

        return logs;
    }
}
