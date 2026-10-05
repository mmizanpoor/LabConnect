using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.TestInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class TestInfoController(ITestInfoService testInfoService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await testInfoService.GetAllAsync(GetUserId());

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(long id)
        => await testInfoService.GetByIdAsync(GetUserId(), id);

    [HttpPost("UpdateApprovePrice")]
    public async Task<OperationResult> UpdateApprovePrice([FromBody] UpdateTestInfoApprovePriceCommand command)
        => await testInfoService.UpdateApprovePriceAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateTestInfoCommand command)
        => await testInfoService.UpdateTestInfoAsync(GetUserId(), command);

    [AllowAnonymous]
    [HttpPost("SaveSendTestInfoResults")]
    public async Task<OperationResult> SaveSendTestInfoResults([FromBody] List<SendTestInfoResult> list)
        => await testInfoService.SaveSendTestInfoResultsAsync(list);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
