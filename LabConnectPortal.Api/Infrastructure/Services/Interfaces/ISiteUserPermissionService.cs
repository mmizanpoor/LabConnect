using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteUserPermissionService
{
    Task<List<SiteUserPermissionDto>> GetForUserAsync(Guid userId);
    Task<List<SiteUserPermissionDto>> GetMemberPermissionsAsync(Guid adminUserId, Guid memberId);
    Task<OperationResult> SetMemberPermissionsAsync(Guid adminUserId, SetSiteMemberPermissionsCommand command);
    Task<bool> HasPermissionAsync(Guid userId, Guid systemEntityId, LabPermissionAction action);
}
