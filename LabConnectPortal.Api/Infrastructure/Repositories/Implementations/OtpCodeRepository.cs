using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class OtpCodeRepository(LabConnectDbContext context) : LabConnectRepository<OtpCode>(context), IOtpCodeRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<OtpCode?> GetValidOtpAsync(string mobileNumber, string code, OtpPurpose purpose)
        => _context.OtpCodes.FirstOrDefaultAsync(o =>
            o.MobileNumber == mobileNumber &&
            o.Code == code &&
            o.Purpose == purpose &&
            !o.IsUsed &&
            o.ExpiresAt > DateTime.UtcNow);

    public async Task InvalidatePreviousAsync(string mobileNumber, OtpPurpose purpose)
    {
        var otps = await _context.OtpCodes
            .Where(o => o.MobileNumber == mobileNumber && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();

        foreach (var otp in otps)
            otp.IsUsed = true;
    }
}
