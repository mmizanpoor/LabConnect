using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.KitGroup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class KitGroupController(IKitGroupService kitGroupService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await kitGroupService.GetAllAsync(GetUserId());

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] CreateKitGroupCommand command)
        => await kitGroupService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateKitGroupCommand command)
        => await kitGroupService.UpdateAsync(GetUserId(), command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(long id)
        => await kitGroupService.DeleteAsync(GetUserId(), id);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
