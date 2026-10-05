using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class ContentGroupController(IContentService contentService) : ControllerBase
{
    [HttpPost("GetAll")]
    public async Task<OperationResult> GetAll([FromBody] GetContentGroupsQuery query)
        => await contentService.GetGroupsAsync(query);

    [HttpGet("GetAllOptions")]
    public async Task<OperationResult> GetAllOptions()
        => await contentService.GetAllGroupsAsync();

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveContentGroupCommand command)
        => await contentService.CreateGroupAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateContentGroupCommand command)
        => await contentService.UpdateGroupAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(int contentGroupId)
        => await contentService.DeleteGroupAsync(contentGroupId);
}

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class ContentPostController(
    IContentService contentService,
    IFileStorageService fileStorageService) : ControllerBase
{
    [HttpPost("GetAll")]
    public async Task<OperationResult> GetAll([FromBody] GetContentPostsQuery query)
        => await contentService.GetPostsAsync(query);

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid contentPostId)
        => await contentService.GetPostByIdAsync(contentPostId);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveContentPostCommand command)
        => await contentService.CreatePostAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateContentPostCommand command)
        => await contentService.UpdatePostAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid contentPostId)
        => await contentService.DeletePostAsync(contentPostId);

    [HttpPost("UploadFeaturedImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadFeaturedImage(Guid contentPostId, IFormFile file)
        => await contentService.UploadFeaturedImageAsync(contentPostId, file);

    [HttpPost("DeleteFeaturedImage")]
    public async Task<OperationResult> DeleteFeaturedImage(Guid contentPostId)
        => await contentService.DeleteFeaturedImageAsync(contentPostId);

    [HttpGet("GetFeaturedImage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedImage(
        [FromServices] IImageThumbnailService imageThumbnails,
        string path,
        int? w = null)
        => await PublicImageResult.FromAsync(
            this,
            imageThumbnails,
            () => fileStorageService.OpenPostFeaturedImageAsync(path),
            path,
            w,
            new OperationResult { Success = false, Message = "تصویر یافت نشد" });
}

[Route("[controller]")]
[ApiController]
public class PublicPostController(IPublicContentService publicContentService) : ControllerBase
{
    [HttpPost("GetPublishedPosts")]
    [AllowAnonymous]
    public async Task<OperationResult> GetPublishedPosts([FromBody] GetPublishedPostsQuery query)
        => await publicContentService.GetPublishedPostsAsync(query);

    [HttpGet("GetHomePosts")]
    [AllowAnonymous]
    public async Task<OperationResult> GetHomePosts(int pageSize = 6)
        => await publicContentService.GetHomePostsAsync(pageSize);

    [HttpGet("GetById")]
    [AllowAnonymous]
    public async Task<OperationResult> GetById(Guid contentPostId)
        => await publicContentService.GetPostDetailAsync(contentPostId);

    [HttpPost("RecordView")]
    [AllowAnonymous]
    public async Task<OperationResult> RecordView(Guid contentPostId)
        => await publicContentService.RecordPostViewAsync(contentPostId);

    [HttpGet("GetPublishedGroups")]
    [AllowAnonymous]
    public async Task<OperationResult> GetPublishedGroups()
        => await publicContentService.GetPublishedGroupsAsync();
}
