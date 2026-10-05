using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class UserProfileController(IUserProfileService userProfileService) : ControllerBase
{
    [HttpGet("GetMyResume")]
    public async Task<OperationResult> GetMyResume()
        => await userProfileService.GetMyResumeAsync(GetUserId());

    [HttpPost("UpdateBasicInfo")]
    public async Task<OperationResult> UpdateBasicInfo([FromBody] UpdateBasicInfoCommand command)
        => await userProfileService.UpdateBasicInfoAsync(GetUserId(), command);

    [HttpPost("UpdateAboutMe")]
    public async Task<OperationResult> UpdateAboutMe([FromBody] UpdateAboutMeCommand command)
        => await userProfileService.UpdateAboutMeAsync(GetUserId(), command);

    [HttpPost("UpdatePersonalInfo")]
    public async Task<OperationResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoCommand command)
        => await userProfileService.UpdatePersonalInfoAsync(GetUserId(), command);

    [HttpPost("UpdateJobPreference")]
    public async Task<OperationResult> UpdateJobPreference([FromBody] UpdateJobPreferenceCommand command)
        => await userProfileService.UpdateJobPreferenceAsync(GetUserId(), command);

    [HttpPost("SaveWorkExperiences")]
    public async Task<OperationResult> SaveWorkExperiences([FromBody] SaveWorkExperiencesCommand command)
        => await userProfileService.SaveWorkExperiencesAsync(GetUserId(), command);

    [HttpPost("SaveEducations")]
    public async Task<OperationResult> SaveEducations([FromBody] SaveEducationsCommand command)
        => await userProfileService.SaveEducationsAsync(GetUserId(), command);

    [HttpPost("SaveSkills")]
    public async Task<OperationResult> SaveSkills([FromBody] SaveSkillsCommand command)
        => await userProfileService.SaveSkillsAsync(GetUserId(), command);

    [HttpPost("SaveLanguages")]
    public async Task<OperationResult> SaveLanguages([FromBody] SaveLanguagesCommand command)
        => await userProfileService.SaveLanguagesAsync(GetUserId(), command);

    [HttpPost("UploadPhoto")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<OperationResult> UploadPhoto(IFormFile file)
        => await userProfileService.UploadPhotoAsync(GetUserId(), file);

    [HttpPost("UploadResume")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<OperationResult> UploadResume(IFormFile file)
        => await userProfileService.UploadResumeAsync(GetUserId(), file);

    [HttpPost("DeleteResume")]
    public async Task<OperationResult> DeleteResume()
        => await userProfileService.DeleteResumeAsync(GetUserId());

    [HttpGet("GetPhoto")]
    public async Task<IActionResult> GetPhoto()
    {
        var (stream, contentType) = await userProfileService.GetPhotoAsync(GetUserId());
        if (stream == null || contentType == null)
            return NotFound();

        return File(stream, contentType);
    }

    [HttpGet("GetResume")]
    public async Task<IActionResult> GetResume()
    {
        var (stream, contentType, fileName) = await userProfileService.GetResumeAsync(GetUserId());
        if (stream == null || contentType == null)
            return NotFound();

        return File(stream, contentType, fileName ?? "resume");
    }

    [HttpGet("GetProvinces")]
    public async Task<OperationResult> GetProvinces()
        => await userProfileService.GetProvincesAsync();

    [HttpGet("GetJobCategories")]
    public async Task<OperationResult> GetJobCategories()
        => await userProfileService.GetJobCategoriesAsync();

    [HttpGet("GetSalaryRanges")]
    public async Task<OperationResult> GetSalaryRanges()
        => await userProfileService.GetSalaryRangesAsync();

    [HttpGet("GetLanguageNames")]
    public async Task<OperationResult> GetLanguageNames()
        => await userProfileService.GetLanguageNamesAsync();

    [HttpGet("SearchSkills")]
    public async Task<OperationResult> SearchSkills([FromQuery] string term)
        => await userProfileService.SearchSkillsAsync(term);

    [HttpPost("CreateSkill")]
    public async Task<OperationResult> CreateSkill([FromBody] CreateSkillCommand command)
        => await userProfileService.CreateSkillAsync(command);

    private Guid GetUserId()
        => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
