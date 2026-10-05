using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CompanyRegulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class CompanyRegulationController(ICompanyRegulationService companyRegulationService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await companyRegulationService.GetAllAsync();

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid companyRegulationId)
        => await companyRegulationService.GetByIdAsync(companyRegulationId);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveCompanyRegulationCommand command)
        => await companyRegulationService.CreateAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateCompanyRegulationCommand command)
        => await companyRegulationService.UpdateAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid companyRegulationId)
        => await companyRegulationService.DeleteAsync(companyRegulationId);
}

[Route("[controller]")]
[ApiController]
public class PublicCompanyRegulationController(IPublicCompanyRegulationService publicCompanyRegulationService) : ControllerBase
{
    [HttpGet("GetAll")]
    [AllowAnonymous]
    public async Task<OperationResult> GetAll()
        => await publicCompanyRegulationService.GetAllAsync();
}
