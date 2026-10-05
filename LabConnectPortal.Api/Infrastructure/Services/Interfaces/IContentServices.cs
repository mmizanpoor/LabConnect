using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Content;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IContentService
{
    Task<OperationResult<PagedResult<ContentGroupDto>>> GetGroupsAsync(GetContentGroupsQuery query);
    Task<OperationResult<List<ContentGroupDto>>> GetAllGroupsAsync();
    Task<OperationResult<ContentGroupDto>> CreateGroupAsync(SaveContentGroupCommand command);
    Task<OperationResult<ContentGroupDto>> UpdateGroupAsync(UpdateContentGroupCommand command);
    Task<OperationResult> DeleteGroupAsync(int contentGroupId);

    Task<OperationResult<PagedResult<ContentPostListItemDto>>> GetPostsAsync(GetContentPostsQuery query);
    Task<OperationResult<ContentPostDto>> GetPostByIdAsync(Guid contentPostId);
    Task<OperationResult<ContentPostDto>> CreatePostAsync(SaveContentPostCommand command);
    Task<OperationResult<ContentPostDto>> UpdatePostAsync(UpdateContentPostCommand command);
    Task<OperationResult> DeletePostAsync(Guid contentPostId);
    Task<OperationResult<ContentPostDto>> UploadFeaturedImageAsync(Guid contentPostId, IFormFile file);
    Task<OperationResult<ContentPostDto>> DeleteFeaturedImageAsync(Guid contentPostId);
}

public interface IPublicContentService
{
    Task<OperationResult<PagedResult<PublicPostCardDto>>> GetPublishedPostsAsync(GetPublishedPostsQuery query);
    Task<OperationResult<List<PublicPostCardDto>>> GetHomePostsAsync(int pageSize = 6);
    Task<OperationResult<PublicPostDetailDto>> GetPostDetailAsync(Guid contentPostId);
    Task<OperationResult> RecordPostViewAsync(Guid contentPostId);
    Task<OperationResult<List<ContentGroupDto>>> GetPublishedGroupsAsync();
}
