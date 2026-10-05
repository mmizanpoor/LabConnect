using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class UserService(
    IUserRepository userRepository,
    ISiteUserPermissionRepository siteUserPermissionRepository,
    ILabUserPermissionRepository labUserPermissionRepository,
    ISiteAnalyticsService siteAnalyticsService) : IUserService
{
    public async Task<OperationResult<PagedResult<UserDto>>> GetUsersAsync(GetUsersQuery query)
    {
        var result = await userRepository.GetPagedAsync(query);
        return OperationResult<PagedResult<UserDto>>.Success(new PagedResult<UserDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        });
    }

    public async Task<OperationResult<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await userRepository.GetWithRolesAsync(id);
        if (user == null)
            return OperationResult<UserDto>.Failure("کاربر یافت نشد");

        return OperationResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult> ToggleActiveAsync(ToggleActiveCommand command)
    {
        var user = await userRepository.GetWithRolesAsync(command.UserId);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        if (IsSystemAdministrator(user))
            return OperationResult.Failure("امکان تغییر وضعیت مدیر سیستم وجود ندارد");

        user.IsActive = command.IsActive;
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var user = await userRepository.GetWithRolesAsync(id);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        if (IsSystemAdministrator(user))
            return OperationResult.Failure("امکان حذف مدیر سیستم وجود ندارد");

        // Track related permissions so Delete is audited before the user row is removed.
        await siteUserPermissionRepository.DeleteByUserIdAsync(id);
        await labUserPermissionRepository.DeleteByUserIdAsync(id);
        userRepository.Delete(user);
        return await userRepository.SaveChangesAsync();
    }

    private static bool IsSystemAdministrator(User user)
        => user.UserType == UserType.Administrator;

    public async Task<OperationResult<UserStatsDto>> GetStatsAsync()
    {
        var stats = await userRepository.GetStatsAsync();

        try
        {
            stats.MonthlySiteVisits = await siteAnalyticsService.GetMonthlyVisitCountAsync();
        }
        catch
        {
            stats.MonthlySiteVisits = 0;
        }

        try
        {
            stats.OnlineUsersCount = await siteAnalyticsService.GetOnlineUsersCountAsync();
        }
        catch
        {
            stats.OnlineUsersCount = 0;
        }

        return OperationResult<UserStatsDto>.Success(stats);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        UserType = user.UserType,
        CenterProfileId = user.CenterProfileId,
        LabCode = user.CenterProfile?.LabCode,
        LabCodeNew = user.CenterProfile?.LabCodeNew,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        MobileNumber = user.MobileNumber,
        IsActive = user.IsActive,
        Address = user.Address,
        Phone = user.Phone,
        Email = user.Email,
        EmailConfirmed = user.EmailConfirmed,
        MobileConfirmed = user.MobileConfirmed,
        CreatedAt = user.CreatedAt,
    };
}
