using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Content;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class PublicContentService(LabConnectDbContext context) : IPublicContentService
{
    public async Task<OperationResult<PagedResult<PublicPostCardDto>>> GetPublishedPostsAsync(GetPublishedPostsQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 12 : Math.Min(query.PageSize, 100);

        var dbQuery = context.ContentPosts.AsNoTracking()
            .Include(p => p.Group)
            .Where(p => p.IsActive);

        if (query.ContentGroupId.HasValue)
            dbQuery = dbQuery.Where(p => p.ContentGroupId == query.ContentGroupId.Value);

        if (query.Type.HasValue)
            dbQuery = dbQuery.Where(p => p.Type == query.Type.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            dbQuery = dbQuery.Where(p =>
                p.Title.Contains(term) ||
                p.ShortDescription.Contains(term) ||
                p.FullBody.Contains(term));
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.PublishedAt ?? p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<PagedResult<PublicPostCardDto>>.Success(new PagedResult<PublicPostCardDto>
        {
            Items = items.Select(MapCard).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<List<PublicPostCardDto>>> GetHomePostsAsync(int pageSize = 6)
    {
        var take = pageSize < 1 ? 6 : Math.Min(pageSize, 20);
        var items = await context.ContentPosts.AsNoTracking()
            .Include(p => p.Group)
            .Where(p => p.IsActive && p.ShowOnHomePage)
            .OrderByDescending(p => p.PublishedAt ?? p.UpdatedAt)
            .Take(take)
            .ToListAsync();

        return OperationResult<List<PublicPostCardDto>>.Success(items.Select(MapCard).ToList());
    }

    public async Task<OperationResult<PublicPostDetailDto>> GetPostDetailAsync(Guid contentPostId)
    {
        var post = await context.ContentPosts.AsNoTracking()
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.ContentPostId == contentPostId && p.IsActive);

        if (post == null)
            return OperationResult<PublicPostDetailDto>.Failure("مطلب یافت نشد");

        return OperationResult<PublicPostDetailDto>.Success(MapDetail(post));
    }

    public async Task<OperationResult> RecordPostViewAsync(Guid contentPostId)
    {
        var post = await context.ContentPosts.FirstOrDefaultAsync(p => p.ContentPostId == contentPostId);
        if (post == null)
            return OperationResult.Failure("مطلب یافت نشد");

        post.ViewCount++;
        await context.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult<List<ContentGroupDto>>> GetPublishedGroupsAsync()
    {
        var items = await context.ContentGroups.AsNoTracking()
            .Where(g => context.ContentPosts.Any(p =>
                p.ContentGroupId == g.ContentGroupId && p.IsActive))
            .OrderBy(g => g.Title)
            .Select(g => new ContentGroupDto
            {
                ContentGroupId = g.ContentGroupId,
                Code = g.Code,
                Title = g.Title,
            })
            .ToListAsync();

        return OperationResult<List<ContentGroupDto>>.Success(items);
    }

    private static PublicPostCardDto MapCard(Domain.Entities.ContentPost post) => new()
    {
        ContentPostId = post.ContentPostId,
        ContentGroupId = post.ContentGroupId,
        GroupTitle = post.Group?.Title ?? string.Empty,
        Type = post.Type,
        TypeTitle = ContentPostTypeHelper.ToPersianTitle(post.Type),
        Title = post.Title,
        ShortDescription = post.ShortDescription,
        FeaturedImagePath = post.FeaturedImagePath,
        ShowAuthor = post.ShowAuthor,
        AuthorName = post.AuthorName,
        PublishedAt = post.PublishedAt,
        ViewCount = post.ViewCount,
        ExternalLink = post.ExternalLink,
    };

    private static PublicPostDetailDto MapDetail(Domain.Entities.ContentPost post) => new()
    {
        ContentPostId = post.ContentPostId,
        ContentGroupId = post.ContentGroupId,
        GroupTitle = post.Group?.Title ?? string.Empty,
        Type = post.Type,
        TypeTitle = ContentPostTypeHelper.ToPersianTitle(post.Type),
        Title = post.Title,
        ShortDescription = post.ShortDescription,
        FullBody = post.FullBody,
        FeaturedImagePath = post.FeaturedImagePath,
        ShowAuthor = post.ShowAuthor,
        AuthorName = post.AuthorName,
        PublishedAt = post.PublishedAt,
        ViewCount = post.ViewCount,
        ExternalLink = post.ExternalLink,
        BrowserTitle = post.BrowserTitle,
        MetaKeywords = post.MetaKeywords,
        MetaDescription = post.MetaDescription,
        CustomMetaTags = post.CustomMetaTags,
    };
}
