using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.DeviceGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class DeviceGroupController(IDeviceGroupService deviceGroupService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await deviceGroupService.GetAllAsync(GetUserId());

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] CreateDeviceGroupCommand command)
        => await deviceGroupService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateDeviceGroupCommand command)
        => await deviceGroupService.UpdateAsync(GetUserId(), command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(long id)
        => await deviceGroupService.DeleteAsync(GetUserId(), id);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
