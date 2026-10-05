using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class SepidRadisanRepository(LabConnectDbContext context) : ISepidRadisanRepository
{
    public Task<SsoAuth?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => context.SsoAuths.FirstOrDefaultAsync(s => s.Username == username, cancellationToken);

    public async Task SaveTokenAsync(string username, string token, DateTime expireDate, CancellationToken cancellationToken = default)
    {
        var existing = await context.SsoAuths.FirstOrDefaultAsync(s => s.Username == username, cancellationToken);

        if (existing != null)
        {
            existing.SessionId = token;
            existing.ExpireDateSup = expireDate;
            existing.Date = DateTime.UtcNow.ToLocalTime();
            context.SsoAuths.Update(existing);
        }
        else
        {
            context.SsoAuths.Add(new SsoAuth
            {
                Username = username,
                SessionId = token,
                ExpireDateSup = expireDate,
                Date = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
