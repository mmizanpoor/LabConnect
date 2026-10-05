namespace LabConnectPortal.Api.Infrastructure.Audit;

internal sealed class PendingActivityChange
{
    public required object Entity { get; init; }
    public required Type ClrType { get; init; }
    public required string EntityName { get; init; }
    public required string Action { get; init; }
    public required string FieldName { get; init; }
    public string? RecordKey { get; set; }
    public string? RecordTitle { get; set; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public Guid? CenterProfileId { get; init; }
    public bool NeedsRecordKey { get; init; }
}
