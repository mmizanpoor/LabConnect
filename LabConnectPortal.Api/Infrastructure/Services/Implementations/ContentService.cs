using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Content;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ContentService(
    LabConnectDbContext context,
    IFileStorageService fileStorageService) : IContentService
{
    public async Task<OperationResult<PagedResult<ContentGroupDto>>> GetGroupsAsync(GetContentGroupsQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.ContentGroups.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(g => g.Title.Contains(query.Title.Trim()));

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderBy(g => g.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new ContentGroupDto
            {
                ContentGroupId = g.ContentGroupId,
                Code = g.Code,
                Title = g.Title,
            })
            .ToListAsync();

        return SuccessPaged(items, totalCount, page, pageSize);
    }

    public async Task<OperationResult<List<ContentGroupDto>>> GetAllGroupsAsync()
    {
        var items = await context.ContentGroups.AsNoTracking()
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

    public async Task<OperationResult<ContentGroupDto>> CreateGroupAsync(SaveContentGroupCommand command)
    {
        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<ContentGroupDto>.Failure("عنوان الزامی است");

        var code = NormalizeCode(command.Code);
        var validation = await ValidateGroupUniquenessAsync(title, code, null);
        if (validation != null)
            return OperationResult<ContentGroupDto>.Failure(validation);

        var entity = new ContentGroup { Title = title, Code = code };
        context.ContentGroups.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ContentGroupDto>.Success(MapGroup(entity));
    }

    public async Task<OperationResult<ContentGroupDto>> UpdateGroupAsync(UpdateContentGroupCommand command)
    {
        var entity = await context.ContentGroups.FirstOrDefaultAsync(g => g.ContentGroupId == command.ContentGroupId);
        if (entity == null)
            return OperationResult<ContentGroupDto>.Failure("گروه یافت نشد");

        var title = command.Title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<ContentGroupDto>.Failure("عنوان الزامی است");

        var code = NormalizeCode(command.Code);
        var validation = await ValidateGroupUniquenessAsync(title, code, command.ContentGroupId);
        if (validation != null)
            return OperationResult<ContentGroupDto>.Failure(validation);

        entity.Title = title;
        entity.Code = code;
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentGroupDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ContentGroupDto>.Success(MapGroup(entity));
    }

    public async Task<OperationResult> DeleteGroupAsync(int contentGroupId)
    {
        var entity = await context.ContentGroups.FirstOrDefaultAsync(g => g.ContentGroupId == contentGroupId);
        if (entity == null)
            return OperationResult.Failure("گروه یافت نشد");

        var inUse = await context.ContentPosts.AnyAsync(p => p.ContentGroupId == contentGroupId);
        if (inUse)
            return OperationResult.Failure("این گروه دارای مطلب است و قابل حذف نیست");

        context.ContentGroups.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<PagedResult<ContentPostListItemDto>>> GetPostsAsync(GetContentPostsQuery query)
    {
        var page = NormalizePage(query.Page);
        var pageSize = NormalizePageSize(query.PageSize);

        var dbQuery = context.ContentPosts.AsNoTracking().Include(p => p.Group).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(p => p.Title.Contains(query.Title.Trim()));
        if (query.ContentGroupId.HasValue)
            dbQuery = dbQuery.Where(p => p.ContentGroupId == query.ContentGroupId.Value);
        if (query.Type.HasValue)
            dbQuery = dbQuery.Where(p => p.Type == query.Type.Value);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(p => p.PublishedAt ?? p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return SuccessPaged(items.Select(MapPostListItem).ToList(), totalCount, page, pageSize);
    }

    public async Task<OperationResult<ContentPostDto>> GetPostByIdAsync(Guid contentPostId)
    {
        var entity = await context.ContentPosts.AsNoTracking()
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.ContentPostId == contentPostId);
        if (entity == null)
            return OperationResult<ContentPostDto>.Failure("مطلب یافت نشد");

        return OperationResult<ContentPostDto>.Success(MapPost(entity));
    }

    public async Task<OperationResult<ContentPostDto>> CreatePostAsync(SaveContentPostCommand command)
    {
        var validation = await ValidatePostCommandAsync(command);
        if (validation != null)
            return OperationResult<ContentPostDto>.Failure(validation);

        var now = DateTime.UtcNow.ToLocalTime();
        var entity = MapCommandToEntity(new ContentPost
        {
            ContentPostId = Guid.NewGuid(),
            CreatedAt = now,
            UpdatedAt = now,
        }, command);

        context.ContentPosts.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentPostDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await context.Entry(entity).Reference(p => p.Group).LoadAsync();
        return OperationResult<ContentPostDto>.Success(MapPost(entity));
    }

    public async Task<OperationResult<ContentPostDto>> UpdatePostAsync(UpdateContentPostCommand command)
    {
        var entity = await context.ContentPosts
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.ContentPostId == command.ContentPostId);
        if (entity == null)
            return OperationResult<ContentPostDto>.Failure("مطلب یافت نشد");

        var validation = await ValidatePostCommandAsync(command);
        if (validation != null)
            return OperationResult<ContentPostDto>.Failure(validation);

        entity = MapCommandToEntity(entity, command);
        entity.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentPostDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await context.Entry(entity).Reference(p => p.Group).LoadAsync();
        return OperationResult<ContentPostDto>.Success(MapPost(entity));
    }

    public async Task<OperationResult> DeletePostAsync(Guid contentPostId)
    {
        var entity = await context.ContentPosts.FirstOrDefaultAsync(p => p.ContentPostId == contentPostId);
        if (entity == null)
            return OperationResult.Failure("مطلب یافت نشد");

        fileStorageService.DeleteFileIfExists(entity.FeaturedImagePath);
        context.ContentPosts.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<ContentPostDto>> UploadFeaturedImageAsync(Guid contentPostId, IFormFile file)
    {
        var entity = await context.ContentPosts
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.ContentPostId == contentPostId);
        if (entity == null)
            return OperationResult<ContentPostDto>.Failure("مطلب یافت نشد");

        var saveResult = await fileStorageService.SavePostFeaturedImageAsync(contentPostId, file, entity.FeaturedImagePath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<ContentPostDto>.Failure(saveResult.Message ?? "خطا در آپلود");

        entity.FeaturedImagePath = saveResult.Data;
        entity.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentPostDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ContentPostDto>.Success(MapPost(entity));
    }

    public async Task<OperationResult<ContentPostDto>> DeleteFeaturedImageAsync(Guid contentPostId)
    {
        var entity = await context.ContentPosts
            .Include(p => p.Group)
            .FirstOrDefaultAsync(p => p.ContentPostId == contentPostId);
        if (entity == null)
            return OperationResult<ContentPostDto>.Failure("مطلب یافت نشد");

        if (string.IsNullOrWhiteSpace(entity.FeaturedImagePath))
            return OperationResult<ContentPostDto>.Failure("تصویری برای این مطلب ثبت نشده است");

        fileStorageService.DeleteFileIfExists(entity.FeaturedImagePath);
        entity.FeaturedImagePath = null;
        entity.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ContentPostDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ContentPostDto>.Success(MapPost(entity));
    }

    private async Task<string?> ValidateGroupUniquenessAsync(string title, string? code, int? excludeId)
    {
        if (await context.ContentGroups.AnyAsync(g =>
                g.Title == title && (!excludeId.HasValue || g.ContentGroupId != excludeId.Value)))
            return "این عنوان قبلاً ثبت شده است";

        if (!string.IsNullOrWhiteSpace(code) && await context.ContentGroups.AnyAsync(g =>
                g.Code == code && (!excludeId.HasValue || g.ContentGroupId != excludeId.Value)))
            return "این شناسه متنی قبلاً ثبت شده است";

        return null;
    }

    private async Task<string?> ValidatePostCommandAsync(SaveContentPostCommand command)
    {
        if (!ContentPostTypeHelper.IsDefined(command.Type))
            return "نوع مطلب نامعتبر است";

        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان الزامی است";

        if (command.Title.Trim().Length > 300)
            return "عنوان نباید بیشتر از ۳۰۰ کاراکتر باشد";

        if ((command.ShortDescription?.Trim().Length ?? 0) > 500)
            return "توضیح کوتاه نباید بیشتر از ۵۰۰ کاراکتر باشد";

        if ((command.AuthorName?.Trim().Length ?? 0) > 200)
            return "نام نویسنده نباید بیشتر از ۲۰۰ کاراکتر باشد";

        if ((command.BrowserTitle?.Trim().Length ?? 0) > 300)
            return "عنوان مرورگر نباید بیشتر از ۳۰۰ کاراکتر باشد";

        if ((command.MetaKeywords?.Trim().Length ?? 0) > 500)
            return "کلمات کلیدی نباید بیشتر از ۵۰۰ کاراکتر باشد";

        if ((command.MetaDescription?.Trim().Length ?? 0) > 500)
            return "توضیحات متا نباید بیشتر از ۵۰۰ کاراکتر باشد";

        if ((command.CustomMetaTags?.Trim().Length ?? 0) > 2000)
            return "متا تگ‌های سفارشی نباید بیشتر از ۲۰۰۰ کاراکتر باشد";

        if ((command.ExternalLink?.Trim().Length ?? 0) > 500)
            return "لینک خارجی نباید بیشتر از ۵۰۰ کاراکتر باشد";

        if (command.ContentGroupId.HasValue &&
            !await context.ContentGroups.AnyAsync(g => g.ContentGroupId == command.ContentGroupId.Value))
            return "گروه نامعتبر است";

        return null;
    }

    private static ContentPost MapCommandToEntity(ContentPost entity, SaveContentPostCommand command)
    {
        entity.ContentGroupId = command.ContentGroupId;
        entity.Type = command.Type;
        entity.Title = command.Title.Trim();
        entity.ShortDescription = command.ShortDescription?.Trim() ?? string.Empty;
        entity.FullBody = command.FullBody ?? string.Empty;
        entity.IsActive = command.IsActive;
        entity.ShowOnHomePage = command.ShowOnHomePage;
        entity.ShowAuthor = command.ShowAuthor;
        entity.AuthorName = string.IsNullOrWhiteSpace(command.AuthorName) ? null : command.AuthorName.Trim();
        entity.BrowserTitle = string.IsNullOrWhiteSpace(command.BrowserTitle) ? null : command.BrowserTitle.Trim();
        entity.MetaKeywords = string.IsNullOrWhiteSpace(command.MetaKeywords) ? null : command.MetaKeywords.Trim();
        entity.MetaDescription = string.IsNullOrWhiteSpace(command.MetaDescription) ? null : command.MetaDescription.Trim();
        entity.CustomMetaTags = string.IsNullOrWhiteSpace(command.CustomMetaTags) ? null : command.CustomMetaTags.Trim();
        entity.ExternalLink = string.IsNullOrWhiteSpace(command.ExternalLink) ? null : command.ExternalLink.Trim();
        entity.ViewCount = Math.Max(0, command.ViewCount);
        entity.PublishedAt = command.PublishedAt ?? entity.PublishedAt ?? DateTime.UtcNow;
        return entity;
    }

    private static string? NormalizeCode(string? code)
    {
        var normalized = code?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ContentGroupDto MapGroup(ContentGroup group) => new()
    {
        ContentGroupId = group.ContentGroupId,
        Code = group.Code,
        Title = group.Title,
    };

    private static ContentPostListItemDto MapPostListItem(ContentPost post) => new()
    {
        ContentPostId = post.ContentPostId,
        ContentGroupId = post.ContentGroupId,
        GroupTitle = post.Group?.Title ?? string.Empty,
        Type = post.Type,
        TypeTitle = ContentPostTypeHelper.ToPersianTitle(post.Type),
        Title = post.Title,
        IsActive = post.IsActive,
        ShowOnHomePage = post.ShowOnHomePage,
        PublishedAt = post.PublishedAt,
        ViewCount = post.ViewCount,
        HasFeaturedImage = !string.IsNullOrWhiteSpace(post.FeaturedImagePath),
        FeaturedImagePath = post.FeaturedImagePath,
    };

    private static ContentPostDto MapPost(ContentPost post) => new()
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
        HasFeaturedImage = !string.IsNullOrWhiteSpace(post.FeaturedImagePath),
        IsActive = post.IsActive,
        ShowOnHomePage = post.ShowOnHomePage,
        ShowAuthor = post.ShowAuthor,
        AuthorName = post.AuthorName,
        PublishedAt = post.PublishedAt,
        BrowserTitle = post.BrowserTitle,
        MetaKeywords = post.MetaKeywords,
        MetaDescription = post.MetaDescription,
        CustomMetaTags = post.CustomMetaTags,
        ExternalLink = post.ExternalLink,
        ViewCount = post.ViewCount,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
    };

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException ex)
        {
            return OperationResult.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize < 1 ? 20 : Math.Min(pageSize, 100);

    private static OperationResult<PagedResult<T>> SuccessPaged<T>(List<T> items, int totalCount, int page, int pageSize)
        => OperationResult<PagedResult<T>>.Success(new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
}
