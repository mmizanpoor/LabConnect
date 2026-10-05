using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ProductReviewService(
    LabConnectDbContext context,
    ICenterProfileRepository centerProfileRepository) : IProductReviewService
{
    public async Task<OperationResult<ProductUserReviewDto>> CreateReviewAsync(Guid userId, CreateProductReviewCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Comment))
            return OperationResult<ProductUserReviewDto>.Failure("متن نظر الزامی است");

        if (command.Rating is < 1 or > 5)
            return OperationResult<ProductUserReviewDto>.Failure("امتیاز باید بین ۱ تا ۵ باشد");

        var orderItem = await context.ProductOrderItems
            .Include(i => i.Order)
            .Include(i => i.UserReview)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == command.OrderItemId);

        if (orderItem == null || orderItem.Order.UserId != userId)
            return OperationResult<ProductUserReviewDto>.Failure("آیتم سفارش یافت نشد");

        if (orderItem.Order.Status is not (ProductOrderStatus.Completed
            or ProductOrderStatus.Shipped
            or ProductOrderStatus.Delivered))
            return OperationResult<ProductUserReviewDto>.Failure("فقط پس از تکمیل سفارش می‌توانید نظر ثبت کنید");

        if (orderItem.UserReview != null)
            return OperationResult<ProductUserReviewDto>.Failure("برای این آیتم قبلاً نظر ثبت شده است");

        if (!orderItem.ProductId.HasValue || orderItem.Product == null)
            return OperationResult<ProductUserReviewDto>.Failure("برای این آیتم امکان ثبت نظر وجود ندارد");

        var user = await context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
        var review = new ProductUserReview
        {
            Id = Guid.NewGuid(),
            OrderItemId = orderItem.Id,
            UserId = userId,
            ProductId = orderItem.ProductId.Value,
            Rating = command.Rating,
            Comment = command.Comment.Trim(),
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
        };

        context.ProductUserReviews.Add(review);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductUserReviewDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductUserReviewDto>.Success(MapReviewDto(review, user, orderItem.Product.Title));
    }

    public async Task<OperationResult<ProductReviewReplyDto>> ReplyAsync(Guid userId, ReplyToReviewCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Comment))
            return OperationResult<ProductReviewReplyDto>.Failure("متن پاسخ الزامی است");

        var review = await context.ProductUserReviews
            .Include(r => r.OrderItem).ThenInclude(i => i.Order)
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);

        if (review == null)
            return OperationResult<ProductReviewReplyDto>.Failure("نظر یافت نشد");

        if (!review.IsApproved)
            return OperationResult<ProductReviewReplyDto>.Failure("فقط به نظرات تأییدشده می‌توانید پاسخ دهید");

        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<ProductReviewReplyDto>.Failure("کاربر یافت نشد");

        var isAdmin = user.UserType == UserType.Administrator;

        var canReply = isAdmin;
        if (!canReply)
        {
            var centerProfile = await ResolveCenterProfileAsync(user);
            canReply = centerProfile != null &&
                       centerProfile.Id == review.OrderItem.Order.CenterProfileId &&
                       (user.UserType is UserType.Store or UserType.AdminLab or UserType.UserLab);
        }

        if (!canReply)
            return OperationResult<ProductReviewReplyDto>.Failure("دسترسی مجاز نیست");

        var hasExistingReply = await context.ProductReviewReplies
            .AnyAsync(r => r.ReviewId == review.Id);
        if (hasExistingReply)
            return OperationResult<ProductReviewReplyDto>.Failure("برای این نظر قبلاً پاسخ ثبت شده است");

        var reply = new ProductReviewReply
        {
            Id = Guid.NewGuid(),
            ReviewId = review.Id,
            UserId = userId,
            Comment = command.Comment.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        context.ProductReviewReplies.Add(reply);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductReviewReplyDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<ProductReviewReplyDto>.Success(new ProductReviewReplyDto
        {
            Id = reply.Id,
            AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
            Comment = reply.Comment,
            CreatedAt = reply.CreatedAt,
        });
    }

    public async Task<OperationResult<ProductReviewEligibilityDto>> GetReviewEligibilityAsync(Guid userId, Guid productId)
    {
        var orderItems = await context.ProductOrderItems
            .AsNoTracking()
            .Include(i => i.Order)
            .Include(i => i.UserReview)
            .Where(i =>
                i.ProductId == productId &&
                i.Order.UserId == userId &&
                (i.Order.Status == ProductOrderStatus.Completed
                    || i.Order.Status == ProductOrderStatus.Shipped
                    || i.Order.Status == ProductOrderStatus.Delivered))
            .ToListAsync();

        if (!orderItems.Any())
        {
            return OperationResult<ProductReviewEligibilityDto>.Success(new ProductReviewEligibilityDto
            {
                CanReview = false,
                HasPendingReview = false,
            });
        }

        var pendingReview = orderItems
            .Select(i => i.UserReview)
            .FirstOrDefault(r => r is { IsApproved: false });

        if (pendingReview != null)
        {
            return OperationResult<ProductReviewEligibilityDto>.Success(new ProductReviewEligibilityDto
            {
                CanReview = false,
                HasPendingReview = true,
            });
        }

        var reviewableItem = orderItems.FirstOrDefault(i => i.UserReview == null);
        return OperationResult<ProductReviewEligibilityDto>.Success(new ProductReviewEligibilityDto
        {
            CanReview = reviewableItem != null,
            HasPendingReview = false,
            OrderItemId = reviewableItem?.Id,
        });
    }

    public async Task<OperationResult<PagedResult<ProductUserReviewDto>>> GetSellerProductReviewsAsync(
        Guid userId,
        GetProductReviewsQuery query)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<PagedResult<ProductUserReviewDto>>.Failure("کاربر یافت نشد");

        var centerProfile = await ResolveCenterProfileAsync(user);
        if (centerProfile == null)
            return OperationResult<PagedResult<ProductUserReviewDto>>.Failure("دسترسی مجاز نیست");

        var ownsProduct = await context.Products.AnyAsync(p =>
            p.CreatedByCenterProfileId == centerProfile.Id && p.ProductId == query.ProductId);
        if (!ownsProduct)
            return OperationResult<PagedResult<ProductUserReviewDto>>.Failure("محصول یافت نشد");

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var reviewsQuery = context.ProductUserReviews.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Include(r => r.Replies).ThenInclude(reply => reply.User)
            .Where(r => r.ProductId == query.ProductId && r.IsApproved);

        var totalCount = await reviewsQuery.CountAsync();
        var reviews = await reviewsQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<PagedResult<ProductUserReviewDto>>.Success(new PagedResult<ProductUserReviewDto>
        {
            Items = reviews.Select(r => MapReviewDto(r, r.User, r.Product.Title, r.Replies)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<PagedResult<ProductReviewListItemDto>>> GetPendingReviewsAsync(
        GetPendingProductReviewsQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var reviewsQuery = context.ProductUserReviews.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => !r.IsApproved);

        var totalCount = await reviewsQuery.CountAsync();
        var reviews = await reviewsQuery
            .OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<PagedResult<ProductReviewListItemDto>>.Success(new PagedResult<ProductReviewListItemDto>
        {
            Items = reviews.Select(r => new ProductReviewListItemDto
            {
                Id = r.Id,
                AuthorName = $"{r.User.FirstName} {r.User.LastName}".Trim(),
                ProductTitle = r.Product.Title,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsApproved = r.IsApproved,
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult> ApproveReviewAsync(Guid adminUserId, ApproveProductReviewCommand command)
    {
        var review = await context.ProductUserReviews
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);

        if (review == null)
            return OperationResult.Failure("نظر یافت نشد");

        if (review.IsApproved)
            return OperationResult.Failure("این نظر قبلاً تأیید شده است");

        review.IsApproved = true;
        review.ApprovedAt = DateTime.UtcNow.ToLocalTime();
        review.ApprovedByUserId = adminUserId;

        return await SaveChangesAsync();
    }

    public async Task<OperationResult> RejectReviewAsync(RejectProductReviewCommand command)
    {
        var review = await context.ProductUserReviews
            .Include(r => r.Replies)
            .FirstOrDefaultAsync(r => r.Id == command.ReviewId);

        if (review == null)
            return OperationResult.Failure("نظر یافت نشد");

        if (review.IsApproved)
            return OperationResult.Failure("نظر تأییدشده قابل رد نیست");

        context.ProductReviewReplies.RemoveRange(review.Replies);
        context.ProductUserReviews.Remove(review);

        return await SaveChangesAsync();
    }

    private static ProductUserReviewDto MapReviewDto(
        ProductUserReview review,
        User user,
        string productTitle,
        IEnumerable<ProductReviewReply>? replies = null)
    {
        var replyList = (replies ?? review.Replies).OrderBy(reply => reply.CreatedAt).ToList();
        return new ProductUserReviewDto
        {
            Id = review.Id,
            AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            IsApproved = review.IsApproved,
            ProductTitle = productTitle,
            Replies = replyList.Select(reply => new ProductReviewReplyDto
            {
                Id = reply.Id,
                AuthorName = reply.User == null
                    ? string.Empty
                    : $"{reply.User.FirstName} {reply.User.LastName}".Trim(),
                Comment = reply.Comment,
                CreatedAt = reply.CreatedAt,
            }).ToList(),
        };
    }

    private async Task<CenterProfile?> ResolveCenterProfileAsync(User user)
    {
        if (user.CenterProfileId.HasValue)
            return await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);

        if (user.UserType == UserType.AdminLab && user.GetLabCode().HasValue)
            return await centerProfileRepository.GetByLabCodeAsync(user.GetLabCode()!.Value);
        if (user.UserType == UserType.Store)
            return await centerProfileRepository.GetByOwnerUserIdAsync(user.Id);
        return null;
    }

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
}
