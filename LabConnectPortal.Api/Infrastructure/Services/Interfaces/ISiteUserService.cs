using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteUserService
{
    Task<OperationResult<List<SiteMemberDto>>> GetMembersAsync(Guid adminUserId);
    Task<OperationResult<AddSiteMemberResultDto>> AddMemberAsync(Guid adminUserId, AddSiteMemberCommand command);
    Task<OperationResult<SiteMemberDto>> ConfirmAddMemberAsync(Guid adminUserId, ConfirmAddSiteMemberCommand command);
    Task<OperationResult<SiteMemberDto>> ToggleMemberActiveAsync(Guid adminUserId, ToggleSiteMemberActiveCommand command);
    Task<OperationResult> RemoveMemberAsync(Guid adminUserId, RemoveSiteMemberCommand command);
}
