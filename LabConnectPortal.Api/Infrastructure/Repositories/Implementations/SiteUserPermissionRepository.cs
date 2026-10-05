using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class SiteUserPermissionRepository(LabConnectDbContext context)
    : LabConnectRepository<SiteUserPermission>(context), ISiteUserPermissionRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<List<SiteUserPermission>> GetByUserIdAsync(Guid userId)
        => _context.SiteUserPermissions.AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync();

    public Task<SiteUserPermission?> GetByUserAndEntityAsync(Guid userId, Guid systemEntityId)
        => _context.SiteUserPermissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SystemEntityId == systemEntityId);

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var permissions = await _context.SiteUserPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();
        _context.SiteUserPermissions.RemoveRange(permissions);
    }
}
