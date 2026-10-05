using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface ISiteUserPermissionRepository : IRepository<SiteUserPermission>
{
    Task<List<SiteUserPermission>> GetByUserIdAsync(Guid userId);
    Task<SiteUserPermission?> GetByUserAndEntityAsync(Guid userId, Guid systemEntityId);
    Task DeleteByUserIdAsync(Guid userId);
}
