using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMobileAsync(string mobileNumber);
    Task<User?> GetByMobileAndUserTypeAsync(string mobileNumber, UserType userType);
    Task<User?> GetByMobileAndUserTypesAsync(string mobileNumber, IEnumerable<UserType> userTypes);
    Task<List<User>> GetByCenterProfileIdAsync(Guid centerProfileId);
    Task<List<User>> GetByUserTypeAsync(UserType userType);
    Task<bool> ExistsByMobileAndCenterProfileIdAsync(string mobileNumber, Guid centerProfileId);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithRolesAsync(Guid id);
    Task<PagedResult<User>> GetPagedAsync(GetUsersQuery query);
    Task<UserStatsDto> GetStatsAsync();
    Task<bool> ExistsByMobileAsync(string mobileNumber);
    Task<bool> ExistsByUsernameAsync(string username);
}
