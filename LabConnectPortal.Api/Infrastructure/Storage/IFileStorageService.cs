using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<OperationResult<string>> SaveCenterProfileFileAsync(
        Guid profileId,
        CenterProfileFileKind kind,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenCenterProfileFileAsync(string relativePath);

    Task<OperationResult<string>> SaveUserProfileFileAsync(
        Guid userId,
        UserProfileFileKind kind,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenUserProfileFileAsync(string relativePath);

    Task<OperationResult<string>> SaveProductImageAsync(
        Guid productId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenProductImageAsync(string relativePath);

    Task<OperationResult<string>> SaveBrandImageAsync(
        int brandId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenBrandImageAsync(string relativePath);

    Task<OperationResult<string>> SaveSiteServiceImageAsync(
        int siteServiceId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenSiteServiceImageAsync(string relativePath);

    Task<OperationResult<string>> SaveCategoryGroupHomePageImageAsync(
        int productCategoryGroupId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenCategoryGroupHomePageImageAsync(string relativePath);

    Task<OperationResult<string>> SavePostFeaturedImageAsync(
        Guid contentPostId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenPostFeaturedImageAsync(string relativePath);

    Task<OperationResult<string>> SaveAdvertisementImageAsync(
        Guid advertisementId,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenAdvertisementImageAsync(string relativePath);

    Task<OperationResult<string>> SaveSiteLogoAsync(IFormFile file, string? existingRelativePath);

    Task<OperationResult<string>> SaveSiteFooterLogoAsync(IFormFile file, string? existingRelativePath);

    Task<OperationResult<string>> SaveSliderSlideImageAsync(Guid sliderGroupId, IFormFile file);

    Task<(Stream? Stream, string? ContentType)> OpenSiteFileAsync(string relativePath);

    Task<OperationResult<string>> SaveLabAgreementSettingsFileAsync(
        Guid centerProfileId,
        LabAgreementSettingsFileKind kind,
        IFormFile file,
        string? existingRelativePath);

    Task<(Stream? Stream, string? ContentType)> OpenLabAgreementSettingsFileAsync(string relativePath);

    void DeleteFileIfExists(string? relativePath);
}
