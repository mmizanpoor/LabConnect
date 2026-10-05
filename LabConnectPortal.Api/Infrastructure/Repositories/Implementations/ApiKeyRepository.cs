using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class ApiKeyRepository(LabConnectDbContext context)
    : LabConnectRepository<ApiKey>(context), IApiKeyRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<List<ApiKey>> GetByCenterProfileIdAsync(Guid centerProfileId)
        => _context.ApiKeys.AsNoTracking()
            .Where(x => x.CenterProfileId == centerProfileId)
            .OrderByDescending(x => x.CreateDate)
            .ToListAsync();

    public Task<List<ApiKey>> GetAllWithCenterAsync()
        => _context.ApiKeys.AsNoTracking()
            .Include(x => x.CenterProfile)
            .OrderByDescending(x => x.CreateDate)
            .ToListAsync();

    public Task<ApiKey?> GetByIdForCenterAsync(Guid id, Guid centerProfileId)
        => _context.ApiKeys.FirstOrDefaultAsync(x =>
            x.Id == id && x.CenterProfileId == centerProfileId);

    public Task<ApiKey?> GetByIdWithCenterAsync(Guid id)
        => _context.ApiKeys.Include(x => x.CenterProfile)
            .FirstOrDefaultAsync(x => x.Id == id);

    public Task<ApiKey?> GetByHashWithCenterAsync(string keyHash)
        => _context.ApiKeys.AsNoTracking()
            .Include(x => x.CenterProfile)
            .FirstOrDefaultAsync(x => x.KeyName == keyHash);

    public Task<bool> ExistsByHashAsync(string keyHash)
        => _context.ApiKeys.AnyAsync(x => x.KeyName == keyHash);
}
