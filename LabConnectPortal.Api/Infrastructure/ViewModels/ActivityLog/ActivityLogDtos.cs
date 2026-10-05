namespace LabConnectPortal.Api.Infrastructure.ViewModels.ActivityLog;

public class ActivityLogUserOptionDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
}

public class GetActivityLogsQuery
{
    public string? EntityName { get; set; }
    public string? RecordKey { get; set; }
    public Guid? CenterProfileId { get; set; }
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ActivityLogChangeDto
{
    public long Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ActivityLogBatchDto
{
    public Guid BatchId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserDisplayName { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public string? RecordTitle { get; set; }
    public Guid? CenterProfileId { get; set; }
    public string? CenterName { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ActivityLogChangeDto> Changes { get; set; } = [];
}

public class ActivityLogRecordGroupDto
{
    public string EntityName { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public string? RecordTitle { get; set; }
    public Guid? CenterProfileId { get; set; }
    public string? CenterName { get; set; }
    public DateTime LatestAt { get; set; }
    public int BatchCount { get; set; }
    public List<ActivityLogBatchDto> Batches { get; set; } = [];
}

public class GetActivityLogsByRecordQuery
{
    public string EntityName { get; set; } = string.Empty;
    public string RecordKey { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
