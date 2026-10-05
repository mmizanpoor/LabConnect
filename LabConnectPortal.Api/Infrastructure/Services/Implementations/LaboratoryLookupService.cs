using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LaboratoryLookupService(ICenterProfileRepository centerProfileRepository)
    : ILaboratoryLookupService
{
    public async Task<OperationResult<LaboratoryLookupDto?>> GetByLabCodeNewAsync(int labCodeNew)
    {
        if (labCodeNew <= 0)
            return OperationResult<LaboratoryLookupDto?>.Success(null);

        var profile = await centerProfileRepository.GetByLabCodeNewAsync(labCodeNew);
        if (profile == null)
            return OperationResult<LaboratoryLookupDto?>.Success(null);

        return OperationResult<LaboratoryLookupDto?>.Success(new LaboratoryLookupDto
        {
            Name = profile.Name,
            LabCode = profile.LabCode,
            LabCodeNew = profile.LabCodeNew ?? labCodeNew,
            IsApproved = profile.IsApproved,
            Status = profile.IsApproved ? "فعال" : "غیرفعال",
        });
    }
}
