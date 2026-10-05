using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILabUserService
{
    Task<OperationResult<List<LabMemberDto>>> GetMembersAsync(Guid adminUserId);
    Task<OperationResult<AddLabMemberResultDto>> AddMemberAsync(Guid adminUserId, AddLabMemberCommand command);
    Task<OperationResult<LabMemberDto>> ConfirmAddMemberAsync(Guid adminUserId, ConfirmAddLabMemberCommand command);
    Task<OperationResult<LabMemberDto>> ToggleMemberActiveAsync(Guid adminUserId, ToggleLabMemberActiveCommand command);
    Task<OperationResult> RemoveMemberAsync(Guid adminUserId, RemoveLabMemberCommand command);
}
