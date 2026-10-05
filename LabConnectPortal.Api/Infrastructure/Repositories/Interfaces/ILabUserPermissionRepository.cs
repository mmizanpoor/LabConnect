using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface ILabUserPermissionRepository : IRepository<LabUserPermission>
{
    Task<List<LabUserPermission>> GetByUserIdAsync(Guid userId);
    Task<LabUserPermission?> GetByUserAndEntityAsync(Guid userId, Guid systemEntityId);
    Task DeleteByUserIdAsync(Guid userId);
}
