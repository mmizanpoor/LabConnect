using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IOtpCodeRepository : IRepository<OtpCode>
{
    Task<OtpCode?> GetValidOtpAsync(string mobileNumber, string code, OtpPurpose purpose);
    Task InvalidatePreviousAsync(string mobileNumber, OtpPurpose purpose);
}
