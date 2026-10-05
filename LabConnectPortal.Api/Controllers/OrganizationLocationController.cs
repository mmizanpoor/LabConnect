using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.OrganizationLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class OrganizationLocationController(IOrganizationLocationService organizationLocationService) : ControllerBase
{
    [HttpGet("GetMyLocations")]
    public async Task<OperationResult> GetMyLocations()
        => await organizationLocationService.GetMyLocationsAsync(GetUserId());

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] CreateOrganizationLocationCommand command)
        => await organizationLocationService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateOrganizationLocationCommand command)
        => await organizationLocationService.UpdateAsync(GetUserId(), command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid locationId)
        => await organizationLocationService.DeleteAsync(GetUserId(), locationId);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
