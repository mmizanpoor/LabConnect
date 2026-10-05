using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class LoginAttemptController(ILoginAttemptService loginAttemptService) : ControllerBase
{
    [HttpPost("GetMine")]
    public async Task<OperationResult> GetMine([FromBody] GetLoginAttemptsQuery query)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await loginAttemptService.GetMineAsync(userId, query);
    }

    [HttpPost("GetAll")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAll([FromBody] GetLoginAttemptsQuery query)
        => await loginAttemptService.GetAllAsync(query);
}
