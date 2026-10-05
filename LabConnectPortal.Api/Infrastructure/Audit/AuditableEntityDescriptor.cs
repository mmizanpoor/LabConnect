using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabConnectPortal.Api.Infrastructure.Audit;

public sealed class AuditableEntityDescriptor
{
    public required Type ClrType { get; init; }
    public required string EntityName { get; init; }
    public required Func<object, string?> GetRecordKey { get; init; }
    public required Func<EntityEntry, DbContext, Guid?> GetCenterProfileId { get; init; }
    public Func<object, string?>? GetRecordTitle { get; init; }
    public IReadOnlySet<string> IgnoredProperties { get; init; } = new HashSet<string>(StringComparer.Ordinal);
}
