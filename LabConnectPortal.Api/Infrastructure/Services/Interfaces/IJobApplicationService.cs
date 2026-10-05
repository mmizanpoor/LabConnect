using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobApplication;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IJobApplicationService
{
    Task<OperationResult<JobApplicationDto>> SubmitAsync(Guid userId, SubmitJobApplicationCommand command);
    Task<OperationResult<List<MyJobApplicationListItemDto>>> GetMyApplicationsAsync(Guid userId);
    Task<OperationResult<PagedResult<JobApplicationDto>>> GetForPostingAsync(Guid userId, GetApplicationsForPostingQuery query);
    Task<OperationResult<JobApplicationDto>> ApproveAsync(Guid userId, ApproveJobApplicationCommand command);
    Task<OperationResult<JobApplicationDto>> RejectAsync(Guid userId, RejectJobApplicationCommand command);
}
