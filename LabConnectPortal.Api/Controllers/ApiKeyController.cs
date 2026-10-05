using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class ApiKeyController(IApiKeyService apiKeyService) : ControllerBase
{
    [HttpGet("GetMine")]
    [AuthorizeUserTypes(UserType.AdminLab, UserType.Store)]
    public async Task<OperationResult> GetMine()
        => await apiKeyService.GetMineAsync(GetUserId());

    [HttpPost("Create")]
    [AuthorizeUserTypes(UserType.AdminLab, UserType.Store)]
    public async Task<OperationResult> Create([FromBody] CreateApiKeyCommand command)
        => await apiKeyService.CreateAsync(GetUserId(), command);

    [HttpPost("UpdateMine")]
    [AuthorizeUserTypes(UserType.AdminLab, UserType.Store)]
    public async Task<OperationResult> UpdateMine([FromBody] UpdateApiKeyCommand command)
        => await apiKeyService.UpdateMineAsync(GetUserId(), command);

    [HttpDelete("DeleteMine")]
    [AuthorizeUserTypes(UserType.AdminLab, UserType.Store)]
    public async Task<OperationResult> DeleteMine(Guid id)
        => await apiKeyService.DeleteMineAsync(GetUserId(), id);

    [HttpGet("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll()
        => await apiKeyService.GetAllAsync();

    [HttpPost("Update")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Update([FromBody] UpdateApiKeyCommand command)
        => await apiKeyService.UpdateAsync(command);

    [HttpDelete("Delete")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Delete(Guid id)
        => await apiKeyService.DeleteAsync(id);

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
