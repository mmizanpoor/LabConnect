using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class UserRepository(LabConnectDbContext context) : LabConnectRepository<User>(context), IUserRepository
{
    private readonly LabConnectDbContext _context = context;

    private IQueryable<User> UsersWithRelations()
        => _context.Users.Include(u => u.CenterProfile);

    public Task<User?> GetByMobileAsync(string mobileNumber)
        => UsersWithRelations().FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber);

    public Task<User?> GetByMobileAndUserTypeAsync(string mobileNumber, UserType userType)
        => UsersWithRelations()
            .FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber && u.UserType == userType);

    public Task<User?> GetByMobileAndUserTypesAsync(string mobileNumber, IEnumerable<UserType> userTypes)
    {
        var types = userTypes.ToList();
        return UsersWithRelations()
            .FirstOrDefaultAsync(u => u.MobileNumber == mobileNumber && types.Contains(u.UserType));
    }

    public Task<List<User>> GetByCenterProfileIdAsync(Guid centerProfileId)
        => UsersWithRelations().AsNoTracking()
            .Where(u => u.CenterProfileId == centerProfileId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    public Task<List<User>> GetByUserTypeAsync(UserType userType)
        => UsersWithRelations().AsNoTracking()
            .Where(u => u.UserType == userType)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    public Task<bool> ExistsByMobileAndCenterProfileIdAsync(string mobileNumber, Guid centerProfileId)
        => _context.Users.AnyAsync(u =>
            u.MobileNumber == mobileNumber && u.CenterProfileId == centerProfileId);

    public Task<User?> GetByUsernameAsync(string username)
    {
        var normalized = (username ?? string.Empty).Trim().ToLower();
        return UsersWithRelations()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var normalized = (email ?? string.Empty).Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalized))
            return Task.FromResult<User?>(null);

        return UsersWithRelations().FirstOrDefaultAsync(u =>
            u.Email != null &&
            u.Email != "" &&
            u.Email.ToLower() == normalized);
    }

    public Task<User?> GetWithRolesAsync(Guid id)
        => UsersWithRelations().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<PagedResult<User>> GetPagedAsync(GetUsersQuery query)
    {
        var q = UsersWithRelations().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            q = q.Where(u =>
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search) ||
                u.MobileNumber.Contains(search) ||
                u.Email.Contains(search) ||
                u.Username.Contains(search));
        }

        if (query.UserType.HasValue)
            q = q.Where(u => u.UserType == query.UserType.Value);

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<UserStatsDto> GetStatsAsync()
    {
        var users = await _context.Users.AsNoTracking().ToListAsync();
        return new UserStatsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            AdministratorCount = users.Count(u => u.UserType == UserType.Administrator),
            LaboratoryCount = users.Count(u => u.UserType == UserType.UserLab),
            UserCount = users.Count(u => u.UserType == UserType.User),
            StoreCount = users.Count(u => u.UserType == UserType.Store),
            AdminLabCount = users.Count(u => u.UserType == UserType.AdminLab)
        };
    }

    public Task<bool> ExistsByMobileAsync(string mobileNumber)
        => _context.Users.AnyAsync(u => u.MobileNumber == mobileNumber);

    public Task<bool> ExistsByUsernameAsync(string username)
    {
        var normalized = (username ?? string.Empty).Trim().ToLower();
        return _context.Users.AnyAsync(u => u.Username.ToLower() == normalized);
    }
}
