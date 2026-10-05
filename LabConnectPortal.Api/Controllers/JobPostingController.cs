using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobPosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Filters;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class JobPostingController(IJobPostingService jobPostingService) : ControllerBase
{
    [HttpPost("GetMyPostings")]
    public async Task<OperationResult> GetMyPostings([FromBody] GetMyJobPostingsQuery query)
        => await jobPostingService.GetMyPostingsAsync(GetUserId(), query);

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid jobPostingId)
        => await jobPostingService.GetByIdAsync(GetUserId(), jobPostingId);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveJobPostingCommand command)
        => await jobPostingService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateJobPostingCommand command)
        => await jobPostingService.UpdateAsync(GetUserId(), command);

    [HttpPost("Publish")]
    public async Task<OperationResult> Publish([FromBody] JobPostingIdCommand command)
        => await jobPostingService.PublishAsync(GetUserId(), command.JobPostingId);

    [HttpPost("Close")]
    public async Task<OperationResult> Close([FromBody] JobPostingIdCommand command)
        => await jobPostingService.CloseAsync(GetUserId(), command.JobPostingId);

    [HttpPost("Reopen")]
    public async Task<OperationResult> Reopen([FromBody] JobPostingIdCommand command)
        => await jobPostingService.ReopenAsync(GetUserId(), command.JobPostingId);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid jobPostingId)
        => await jobPostingService.DeleteAsync(GetUserId(), jobPostingId);

    [HttpGet("GetDashboardStats")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> GetDashboardStats()
        => await jobPostingService.GetDashboardStatsAsync();

    [HttpGet("GetDashboardStatsForCurrentLab")]
    [SkipSystemEntity]
    public async Task<OperationResult> GetDashboardStatsForCurrentLab()
        => await jobPostingService.GetDashboardStatsForCurrentLabAsync(GetUserId());

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
