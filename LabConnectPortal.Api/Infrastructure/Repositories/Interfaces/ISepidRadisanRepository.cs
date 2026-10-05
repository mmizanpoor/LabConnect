using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface ISepidRadisanRepository
{
    Task<SsoAuth?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task SaveTokenAsync(string username, string token, DateTime expireDate, CancellationToken cancellationToken = default);
}
