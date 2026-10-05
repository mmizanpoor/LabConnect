using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;
namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILabUserPermissionService
{
    Task<List<LabUserPermissionDto>> GetForUserAsync(Guid userId);
    Task<List<LabUserPermissionDto>> GetMemberPermissionsAsync(Guid adminUserId, Guid memberId);
    Task<OperationResult> SetMemberPermissionsAsync(Guid adminUserId, SetLabMemberPermissionsCommand command);
    Task<bool> HasPermissionAsync(Guid userId, Guid systemEntityId, LabPermissionAction action);
}
