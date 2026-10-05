using LabConnectPortal.Api.Domain;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteUserPermissionService(
    IUserRepository userRepository,
    ISiteUserPermissionRepository permissionRepository) : ISiteUserPermissionService
{
    public async Task<List<SiteUserPermissionDto>> GetForUserAsync(Guid userId)
    {
        var permissions = await permissionRepository.GetByUserIdAsync(userId);
        return BuildPermissionMatrix(permissions);
    }

    public async Task<List<SiteUserPermissionDto>> GetMemberPermissionsAsync(Guid adminUserId, Guid memberId)
    {
        var adminResult = await GetAdministratorOrFailure(adminUserId);
        if (adminResult.Error != null)
            return [];

        var memberResult = await GetManagedMemberOrFailure(memberId);
        if (memberResult.Error != null)
            return [];

        var permissions = await permissionRepository.GetByUserIdAsync(memberId);
        return BuildPermissionMatrix(permissions);
    }

    public async Task<OperationResult> SetMemberPermissionsAsync(
        Guid adminUserId,
        SetSiteMemberPermissionsCommand command)
    {
        var adminResult = await GetAdministratorOrFailure(adminUserId);
        if (adminResult.Error != null)
            return OperationResult.Failure(adminResult.Error);

        var memberResult = await GetManagedMemberOrFailure(command.MemberId);
        if (memberResult.Error != null)
            return OperationResult.Failure(memberResult.Error);

        var existing = await permissionRepository.GetByUserIdAsync(command.MemberId);
        foreach (var item in existing)
            permissionRepository.Delete(item);

        foreach (var dto in command.Permissions.Where(p => SystemEntity.IsSiteAdminEntity(p.SystemEntityId)))
        {
            if (!dto.CanView && !dto.CanCreate && !dto.CanUpdate && !dto.CanDelete)
                continue;

            permissionRepository.Add(new SiteUserPermission
            {
                Id = Guid.NewGuid(),
                UserId = command.MemberId,
                SystemEntityId = dto.SystemEntityId,
                CanView = dto.CanView,
                CanCreate = dto.CanCreate,
                CanUpdate = dto.CanUpdate,
                CanDelete = dto.CanDelete,
            });
        }

        return await permissionRepository.SaveChangesAsync();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid systemEntityId, LabPermissionAction action)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        if (user.UserType == UserType.Administrator)
            return true;

        if (user.UserType != UserType.Admin)
            return false;

        var permission = await permissionRepository.GetByUserAndEntityAsync(userId, systemEntityId);
        if (permission == null)
            return false;

        return action switch
        {
            LabPermissionAction.View => permission.CanView,
            LabPermissionAction.Create => permission.CanCreate,
            LabPermissionAction.Update => permission.CanUpdate,
            LabPermissionAction.Delete => permission.CanDelete,
            _ => false,
        };
    }

    private static List<SiteUserPermissionDto> BuildPermissionMatrix(IEnumerable<SiteUserPermission> permissions)
    {
        var byEntity = permissions.ToDictionary(p => p.SystemEntityId);
        return SystemEntity.SiteAdminAll.Select(entityId =>
        {
            if (byEntity.TryGetValue(entityId, out var permission))
            {
                return new SiteUserPermissionDto
                {
                    SystemEntityId = entityId,
                    CanView = permission.CanView,
                    CanCreate = permission.CanCreate,
                    CanUpdate = permission.CanUpdate,
                    CanDelete = permission.CanDelete,
                };
            }

            return new SiteUserPermissionDto { SystemEntityId = entityId };
        }).ToList();
    }

    private async Task<(User? User, string? Error)> GetAdministratorOrFailure(Guid adminUserId)
    {
        var admin = await userRepository.GetByIdAsync(adminUserId);
        if (admin == null || admin.UserType != UserType.Administrator)
            return (null, "دسترسی مجاز نیست");

        return (admin, null);
    }

    private async Task<(User? Member, string? Error)> GetManagedMemberOrFailure(Guid memberId)
    {
        var member = await userRepository.GetByIdAsync(memberId);
        if (member == null)
            return (null, "کاربر یافت نشد");

        if (member.UserType != UserType.Admin)
            return (null, "تنها برای کاربران سایت می‌توان دسترسی تعریف کرد");

        return (member, null);
    }
}
