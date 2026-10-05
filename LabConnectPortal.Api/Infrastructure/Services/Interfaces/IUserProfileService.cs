using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IUserProfileService
{
    Task<OperationResult<ResumeDto>> GetMyResumeAsync(Guid userId);
    Task<OperationResult<ResumeDto>> UpdateBasicInfoAsync(Guid userId, UpdateBasicInfoCommand command);
    Task<OperationResult<ResumeDto>> UpdateAboutMeAsync(Guid userId, UpdateAboutMeCommand command);
    Task<OperationResult<ResumeDto>> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoCommand command);
    Task<OperationResult<ResumeDto>> UpdateJobPreferenceAsync(Guid userId, UpdateJobPreferenceCommand command);
    Task<OperationResult<ResumeDto>> SaveWorkExperiencesAsync(Guid userId, SaveWorkExperiencesCommand command);
    Task<OperationResult<ResumeDto>> SaveEducationsAsync(Guid userId, SaveEducationsCommand command);
    Task<OperationResult<ResumeDto>> SaveSkillsAsync(Guid userId, SaveSkillsCommand command);
    Task<OperationResult<ResumeDto>> SaveLanguagesAsync(Guid userId, SaveLanguagesCommand command);
    Task<OperationResult<ResumeDto>> UploadPhotoAsync(Guid userId, IFormFile file);
    Task<OperationResult<ResumeDto>> UploadResumeAsync(Guid userId, IFormFile file);
    Task<OperationResult<ResumeDto>> DeleteResumeAsync(Guid userId);
    Task<(Stream? Stream, string? ContentType)> GetPhotoAsync(Guid userId);
    Task<(Stream? Stream, string? ContentType, string? FileName)> GetResumeAsync(Guid userId);
    Task<OperationResult<List<ReferenceItemDto>>> GetProvincesAsync();
    Task<OperationResult<List<ReferenceItemDto>>> GetJobCategoriesAsync();
    Task<OperationResult<List<ReferenceItemDto>>> GetSalaryRangesAsync();
    Task<OperationResult<List<ReferenceItemDto>>> GetLanguageNamesAsync();
    Task<OperationResult<List<SkillSearchResultDto>>> SearchSkillsAsync(string term);
    Task<OperationResult<SkillSearchResultDto>> CreateSkillAsync(CreateSkillCommand command);
}
