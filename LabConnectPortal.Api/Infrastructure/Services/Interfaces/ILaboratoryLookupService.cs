using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILaboratoryLookupService
{
    Task<OperationResult<LaboratoryLookupDto?>> GetByLabCodeNewAsync(int labCodeNew);
}
