using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.PublicJob;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IPublicJobService
{
    Task<OperationResult<PagedResult<PublicJobPostingCardDto>>> GetActivePostingsAsync(GetActivePostingsQuery query);
    Task<OperationResult<PublicJobPostingDetailDto>> GetByIdAsync(Guid jobPostingId);
    Task<OperationResult<PublicJobPostingFiltersDto>> GetPostingFiltersAsync();
}
