using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Content;

public class ContentGroupDto
{
    public int ContentGroupId { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class SaveContentGroupCommand
{
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class UpdateContentGroupCommand : SaveContentGroupCommand
{
    public int ContentGroupId { get; set; }
}

public class GetContentGroupsQuery
{
    public string? Title { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ContentPostListItemDto
{
    public Guid ContentPostId { get; set; }
    public int? ContentGroupId { get; set; }
    public string GroupTitle { get; set; } = string.Empty;
    public ContentPostType Type { get; set; }
    public string TypeTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool ShowOnHomePage { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public bool HasFeaturedImage { get; set; }
    public string? FeaturedImagePath { get; set; }
}

public class ContentPostDto
{
    public Guid ContentPostId { get; set; }
    public int? ContentGroupId { get; set; }
    public string GroupTitle { get; set; } = string.Empty;
    public ContentPostType Type { get; set; }
    public string TypeTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public string? FeaturedImagePath { get; set; }
    public bool HasFeaturedImage { get; set; }
    public bool IsActive { get; set; }
    public bool ShowOnHomePage { get; set; }
    public bool ShowAuthor { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? BrowserTitle { get; set; }
    public string? MetaKeywords { get; set; }
    public string? MetaDescription { get; set; }
    public string? CustomMetaTags { get; set; }
    public string? ExternalLink { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SaveContentPostCommand
{
    public int? ContentGroupId { get; set; }
    public ContentPostType Type { get; set; } = ContentPostType.News;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool ShowOnHomePage { get; set; }
    public bool ShowAuthor { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? BrowserTitle { get; set; }
    public string? MetaKeywords { get; set; }
    public string? MetaDescription { get; set; }
    public string? CustomMetaTags { get; set; }
    public string? ExternalLink { get; set; }
    public int ViewCount { get; set; }
}

public class UpdateContentPostCommand : SaveContentPostCommand
{
    public Guid ContentPostId { get; set; }
}

public class GetContentPostsQuery
{
    public string? Title { get; set; }
    public int? ContentGroupId { get; set; }
    public ContentPostType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetPublishedPostsQuery
{
    public int? ContentGroupId { get; set; }
    public ContentPostType? Type { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PublicPostCardDto
{
    public Guid ContentPostId { get; set; }
    public int? ContentGroupId { get; set; }
    public string GroupTitle { get; set; } = string.Empty;
    public ContentPostType Type { get; set; }
    public string TypeTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? FeaturedImagePath { get; set; }
    public bool ShowAuthor { get; set; }
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public string? ExternalLink { get; set; }
}

public class PublicPostDetailDto : PublicPostCardDto
{
    public string FullBody { get; set; } = string.Empty;
    public string? BrowserTitle { get; set; }
    public string? MetaKeywords { get; set; }
    public string? MetaDescription { get; set; }
    public string? CustomMetaTags { get; set; }
}
