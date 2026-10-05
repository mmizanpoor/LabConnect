using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class JobApplicationController(IJobApplicationService jobApplicationService) : ControllerBase
{
    [HttpPost("Submit")]
    public async Task<OperationResult> Submit([FromBody] SubmitJobApplicationCommand command)
        => await jobApplicationService.SubmitAsync(GetUserId(), command);

    [HttpGet("GetMyApplications")]
    public async Task<OperationResult> GetMyApplications()
        => await jobApplicationService.GetMyApplicationsAsync(GetUserId());

    [HttpPost("GetForPosting")]
    public async Task<OperationResult> GetForPosting([FromBody] GetApplicationsForPostingQuery query)
        => await jobApplicationService.GetForPostingAsync(GetUserId(), query);

    [HttpPost("Approve")]
    public async Task<OperationResult> Approve([FromBody] ApproveJobApplicationCommand command)
        => await jobApplicationService.ApproveAsync(GetUserId(), command);

    [HttpPost("Reject")]
    public async Task<OperationResult> Reject([FromBody] RejectJobApplicationCommand command)
        => await jobApplicationService.RejectAsync(GetUserId(), command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
