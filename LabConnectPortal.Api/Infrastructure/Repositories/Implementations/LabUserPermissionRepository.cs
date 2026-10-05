using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class LabUserPermissionRepository(LabConnectDbContext context)
    : LabConnectRepository<LabUserPermission>(context), ILabUserPermissionRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<List<LabUserPermission>> GetByUserIdAsync(Guid userId)
        => _context.LabUserPermissions.AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync();

    public Task<LabUserPermission?> GetByUserAndEntityAsync(Guid userId, Guid systemEntityId)
        => _context.LabUserPermissions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SystemEntityId == systemEntityId);

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var permissions = await _context.LabUserPermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();
        _context.LabUserPermissions.RemoveRange(permissions);
    }
}
