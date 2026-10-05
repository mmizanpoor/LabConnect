using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
[SkipSystemEntity]
public class PublicLaboratoryController(
    ILaboratoryLookupService laboratoryLookupService,
    IReceptionService receptionService) : ControllerBase
{
    [HttpGet("GetByLabCodeNew")]
    public async Task<OperationResult> GetByLabCodeNew([FromQuery] int labCodeNew)
        => await laboratoryLookupService.GetByLabCodeNewAsync(labCodeNew);

    [HttpGet("GetLabDetailAsync")]
    public async Task<OperationResult> GetLabDetailAsync([FromQuery] int labCodeNew)
        => await receptionService.GetLabDetailAsync(labCodeNew);
}
