using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteChargeService;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class SiteChargeServiceController(ISiteChargeServiceAdminService service) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await service.GetAllAsync();

    [HttpPost("Save")]
    public async Task<OperationResult> Save([FromBody] SaveSiteChargeServiceCommand command)
        => await service.SaveAsync(GetUserId(), command);

    [HttpPost("Delete")]
    public async Task<OperationResult> Delete([FromBody] DeleteSiteChargeServiceCommand command)
        => await service.DeleteAsync(command.SiteChargeServiceId);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
