using LabConnectPortal.Api.Domain;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LabUserPermissionService(
    IUserRepository userRepository,
    ILabUserPermissionRepository permissionRepository) : ILabUserPermissionService
{
    public async Task<List<LabUserPermissionDto>> GetForUserAsync(Guid userId)
    {
        var permissions = await permissionRepository.GetByUserIdAsync(userId);
        return BuildPermissionMatrix(permissions);
    }

    public async Task<List<LabUserPermissionDto>> GetMemberPermissionsAsync(Guid adminUserId, Guid memberId)
    {
        var adminResult = await GetAdminOrFailure(adminUserId);
        if (adminResult.Error != null)
            return [];

        var memberResult = await GetManagedMemberOrFailure(adminResult.User!, memberId);
        if (memberResult.Error != null)
            return [];

        var permissions = await permissionRepository.GetByUserIdAsync(memberId);
        return BuildPermissionMatrix(permissions);
    }

    public async Task<OperationResult> SetMemberPermissionsAsync(
        Guid adminUserId,
        SetLabMemberPermissionsCommand command)
    {
        var adminResult = await GetAdminOrFailure(adminUserId);
        if (adminResult.Error != null)
            return OperationResult.Failure(adminResult.Error);

        var memberResult = await GetManagedMemberOrFailure(adminResult.User!, command.MemberId);
        if (memberResult.Error != null)
            return OperationResult.Failure(memberResult.Error);

        var existing = await permissionRepository.GetByUserIdAsync(command.MemberId);
        foreach (var item in existing)
            permissionRepository.Delete(item);

        foreach (var dto in command.Permissions.Where(p => SystemEntity.IsLabPortalEntity(p.SystemEntityId)))
        {
            if (!dto.CanView && !dto.CanCreate && !dto.CanUpdate && !dto.CanDelete)
                continue;

            permissionRepository.Add(new LabUserPermission
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

        if (user.UserType is UserType.Administrator or UserType.AdminLab)
            return true;

        if (user.UserType != UserType.UserLab)
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

    private static List<LabUserPermissionDto> BuildPermissionMatrix(IEnumerable<LabUserPermission> permissions)
    {
        var byEntity = permissions.ToDictionary(p => p.SystemEntityId);
        return SystemEntity.LabPortalAll.Select(entityId =>
        {
            if (byEntity.TryGetValue(entityId, out var permission))
            {
                return new LabUserPermissionDto
                {
                    SystemEntityId = entityId,
                    CanView = permission.CanView,
                    CanCreate = permission.CanCreate,
                    CanUpdate = permission.CanUpdate,
                    CanDelete = permission.CanDelete,
                };
            }

            return new LabUserPermissionDto { SystemEntityId = entityId };
        }).ToList();
    }

    private async Task<(User? User, string? Error)> GetAdminOrFailure(Guid adminUserId)
    {
        var admin = await userRepository.GetByIdAsync(adminUserId);
        if (admin == null || admin.UserType != UserType.AdminLab)
            return (null, "دسترسی مجاز نیست");

        if (!admin.CenterProfileId.HasValue)
            return (null, "پروفایل مرکز برای حساب کاربری تعریف نشده است");

        return (admin, null);
    }

    private async Task<(User? Member, string? Error)> GetManagedMemberOrFailure(User admin, Guid memberId)
    {
        var member = await userRepository.GetByIdAsync(memberId);
        if (member == null)
            return (null, "کاربر یافت نشد");

        if (member.UserType != UserType.UserLab)
            return (null, "تنها برای کاربران آزمایشگاه می‌توان دسترسی تعریف کرد");

        if (member.CenterProfileId != admin.CenterProfileId)
            return (null, "کاربر متعلق به این آزمایشگاه نیست");

        return (member, null);
    }
}
