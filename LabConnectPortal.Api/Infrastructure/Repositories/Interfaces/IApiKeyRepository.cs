using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IApiKeyRepository : IRepository<ApiKey>
{
    Task<List<ApiKey>> GetByCenterProfileIdAsync(Guid centerProfileId);
    Task<List<ApiKey>> GetAllWithCenterAsync();
    Task<ApiKey?> GetByIdForCenterAsync(Guid id, Guid centerProfileId);
    Task<ApiKey?> GetByIdWithCenterAsync(Guid id);
    Task<ApiKey?> GetByHashWithCenterAsync(string keyHash);
    Task<bool> ExistsByHashAsync(string keyHash);
}
