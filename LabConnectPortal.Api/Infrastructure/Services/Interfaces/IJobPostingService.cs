using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobPosting;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IJobPostingService
{
    Task<OperationResult<PagedResult<JobPostingListItemDto>>> GetMyPostingsAsync(Guid userId, GetMyJobPostingsQuery query);
    Task<OperationResult<JobPostingDto>> GetByIdAsync(Guid userId, Guid jobPostingId);
    Task<OperationResult<JobPostingDto>> CreateAsync(Guid userId, SaveJobPostingCommand command);
    Task<OperationResult<JobPostingDto>> UpdateAsync(Guid userId, UpdateJobPostingCommand command);
    Task<OperationResult<JobPostingDto>> PublishAsync(Guid userId, Guid jobPostingId);
    Task<OperationResult<JobPostingDto>> CloseAsync(Guid userId, Guid jobPostingId);
    Task<OperationResult<JobPostingDto>> ReopenAsync(Guid userId, Guid jobPostingId);
    Task<OperationResult> DeleteAsync(Guid userId, Guid jobPostingId);
    Task<OperationResult<JobPostingDashboardStatsDto>> GetDashboardStatsAsync();
    Task<OperationResult<JobPostingDashboardStatsDto>> GetDashboardStatsForCurrentLabAsync(Guid userId);
}
