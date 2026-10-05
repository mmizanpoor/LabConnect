using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILoginAttemptService
{
    Task RecordAsync(RecordLoginAttemptCommand command);

    Task<OperationResult<PagedResult<UserLoginLogDto>>> GetMineAsync(
        Guid userId,
        GetLoginAttemptsQuery query);

    Task<OperationResult<PagedResult<UserLoginLogDto>>> GetAllAsync(GetLoginAttemptsQuery query);
}
