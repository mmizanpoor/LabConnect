using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IUserService
{
    Task<OperationResult<PagedResult<UserDto>>> GetUsersAsync(GetUsersQuery query);
    Task<OperationResult<UserDto>> GetByIdAsync(Guid id);
    Task<OperationResult> ToggleActiveAsync(ToggleActiveCommand command);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<UserStatsDto>> GetStatsAsync();
}
