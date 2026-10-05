using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CompanyRegulation;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ICompanyRegulationService
{
    Task<OperationResult<List<CompanyRegulationDto>>> GetAllAsync();
    Task<OperationResult<CompanyRegulationDto>> GetByIdAsync(Guid companyRegulationId);
    Task<OperationResult<CompanyRegulationDto>> CreateAsync(SaveCompanyRegulationCommand command);
    Task<OperationResult<CompanyRegulationDto>> UpdateAsync(UpdateCompanyRegulationCommand command);
    Task<OperationResult> DeleteAsync(Guid companyRegulationId);
}

public interface IPublicCompanyRegulationService
{
    Task<OperationResult<List<CompanyRegulationDto>>> GetAllAsync();
}
