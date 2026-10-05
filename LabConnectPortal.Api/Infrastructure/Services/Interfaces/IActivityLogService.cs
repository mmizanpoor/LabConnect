using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ActivityLog;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IActivityLogService
{
    Task<OperationResult<PagedResult<ActivityLogRecordGroupDto>>> GetGroupedAsync(Guid requesterUserId, GetActivityLogsQuery query);

    Task<OperationResult<PagedResult<ActivityLogBatchDto>>> GetBatchesAsync(Guid requesterUserId, GetActivityLogsQuery query);

    Task<OperationResult<PagedResult<ActivityLogBatchDto>>> GetByRecordAsync(Guid requesterUserId, GetActivityLogsByRecordQuery query);

    Task<OperationResult<ActivityLogBatchDto>> GetBatchAsync(Guid requesterUserId, Guid batchId);

    Task<OperationResult<List<ActivityLogUserOptionDto>>> GetCenterUsersAsync(Guid requesterUserId);
}
