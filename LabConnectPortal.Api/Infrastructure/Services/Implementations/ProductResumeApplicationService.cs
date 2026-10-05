using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductResumeApplication;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ProductResumeApplicationService(
    LabConnectDbContext context,
    IUserProfileRepository userProfileRepository,
    IUserProfileService userProfileService,
    INotificationService notificationService) : IProductResumeApplicationService
{
    public async Task<OperationResult<ProductResumeApplicationDto>> SubmitAsync(Guid userId, SubmitProductResumeCommand command)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<ProductResumeApplicationDto>.Failure("کاربر یافت نشد");

        if (user.UserType != UserType.User)
            return OperationResult<ProductResumeApplicationDto>.Failure("فقط کاربران عادی می‌توانند رزومه ارسال کنند");

        var product = await context.Products
            .AsNoTracking()
            .Include(p => p.CreatedByCenterProfile)
            .Include(p => p.CategoryAssignments)
                .ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(p =>
                p.ProductId == command.ProductId &&
                p.Status == ProductStatus.Approved);

        if (product == null)
            return OperationResult<ProductResumeApplicationDto>.Failure("آگهی یافت نشد");

        if (!product.CategoryAssignments.Any(a => a.Category != null && a.Category.AcceptsResume))
            return OperationResult<ProductResumeApplicationDto>.Failure("این آگهی امکان دریافت رزومه ندارد");

        if (product.CreatedByUserId == userId)
            return OperationResult<ProductResumeApplicationDto>.Failure("امکان ارسال رزومه برای آگهی خودتان وجود ندارد");

        var userWithResume = await userProfileRepository.GetUserWithResumeAsync(userId);
        if (userWithResume == null)
            return OperationResult<ProductResumeApplicationDto>.Failure("کاربر یافت نشد");

        if (!ResumeCompletenessHelper.IsCompleteForApplication(userWithResume))
            return OperationResult<ProductResumeApplicationDto>.Failure("برای ارسال رزومه، اطلاعات پایه رزومه باید تکمیل شود");

        var existing = await context.ProductResumeApplications
            .AsNoTracking()
            .AnyAsync(a => a.ProductId == command.ProductId && a.ApplicantUserId == userId);
        if (existing)
            return OperationResult<ProductResumeApplicationDto>.Failure("رزومه شما قبلاً برای این آگهی ارسال شده است");

        var application = new ProductResumeApplication
        {
            ProductResumeApplicationId = Guid.NewGuid(),
            ProductId = command.ProductId,
            ApplicantUserId = userId,
            Status = ProductResumeApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
        };

        context.ProductResumeApplications.Add(application);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return OperationResult<ProductResumeApplicationDto>.Failure(ex.Message);
        }

        var applicantName = $"{userWithResume.FirstName} {userWithResume.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(applicantName))
            applicantName = userWithResume.MobileNumber;

        var recipientId = product.CreatedByUserId
            ?? product.CreatedByCenterProfile?.OwnerUserId;
        if (recipientId.HasValue && recipientId.Value != Guid.Empty)
        {
            var title = "رزومه جدید برای آگهی";
            var message = $"کاربر {applicantName} برای آگهی «{product.Title}» رزومه ارسال کرد.";
            await notificationService.CreateAsync(
                recipientId.Value,
                title,
                message,
                NotificationType.General);

            await notificationService.SendExternalNotificationAsync(
                new NotificationCommandBase
                {
                    Title = title,
                    Message = message,
                    IsActive = true,
                    Priority = 1,
                    NotificationActions =
                    [
                        new NotificationAction
                        {
                            NotificationActionType = NotificationActionType.OpenUrl,
                            Label = "مشاهده آگهی",
                            Url = $"/products/{product.ProductId}",
                        },
                    ],
                },
                [recipientId.Value]);
        }

        return OperationResult<ProductResumeApplicationDto>.Success(Map(application, product.Title));
    }

    public async Task<OperationResult<List<ProductResumeApplicationDto>>> GetMyApplicationsAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<List<ProductResumeApplicationDto>>.Failure("کاربر یافت نشد");

        if (user.UserType != UserType.User)
            return OperationResult<List<ProductResumeApplicationDto>>.Failure("فقط کاربران عادی می‌توانند رزومه‌های ارسالی را مشاهده کنند");

        var applications = await context.ProductResumeApplications
            .AsNoTracking()
            .Include(a => a.Product)
            .Where(a => a.ApplicantUserId == userId)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        return OperationResult<List<ProductResumeApplicationDto>>.Success(
            applications.Select(a => Map(a, a.Product.Title)).ToList());
    }

    public async Task<OperationResult<ProductResumeApplicationsPageDto>> GetForProductAsync(
        Guid userId,
        GetProductResumeApplicationsQuery query)
    {
        var product = await LoadProductForReviewAsync(query.ProductId);
        if (product == null)
            return OperationResult<ProductResumeApplicationsPageDto>.Failure("آگهی یافت نشد");

        if (!await CanReviewProductAsync(userId, product))
            return OperationResult<ProductResumeApplicationsPageDto>.Failure("دسترسی غیرمجاز");

        if (!product.CategoryAssignments.Any(a => a.Category != null && a.Category.AcceptsResume))
            return OperationResult<ProductResumeApplicationsPageDto>.Failure("این آگهی امکان دریافت رزومه ندارد");

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.ProductResumeApplications
            .AsNoTracking()
            .Include(a => a.ApplicantUser)
                .ThenInclude(u => u.Profile)
            .Include(a => a.ApplicantUser)
                .ThenInclude(u => u.EducationalBackgrounds)
            .Where(a => a.ProductId == query.ProductId);

        var totalCount = await dbQuery.CountAsync();
        var applications = await dbQuery
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<ProductResumeApplicationsPageDto>.Success(
            new ProductResumeApplicationsPageDto
            {
                ProductTitle = product.Title,
                Items = applications.Select(a => MapForOwner(a, product.Title)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            });
    }

    public async Task<OperationResult<ResumeDto>> GetApplicantResumeAsync(Guid userId, Guid productResumeApplicationId)
    {
        var access = await GetReviewableApplicationAsync(userId, productResumeApplicationId);
        if (access.Error != null)
            return OperationResult<ResumeDto>.Failure(access.Error);

        return await userProfileService.GetMyResumeAsync(access.Application!.ApplicantUserId);
    }

    public async Task<(Stream? Stream, string? ContentType, string? Error)> GetApplicantResumePhotoAsync(
        Guid userId,
        Guid productResumeApplicationId)
    {
        var access = await GetReviewableApplicationAsync(userId, productResumeApplicationId);
        if (access.Error != null)
            return (null, null, access.Error);

        var (stream, contentType) = await userProfileService.GetPhotoAsync(access.Application!.ApplicantUserId);
        return (stream, contentType, null);
    }

    public async Task<(Stream? Stream, string? ContentType, string? FileName, string? Error)> GetApplicantResumeFileAsync(
        Guid userId,
        Guid productResumeApplicationId)
    {
        var access = await GetReviewableApplicationAsync(userId, productResumeApplicationId);
        if (access.Error != null)
            return (null, null, null, access.Error);

        var (stream, contentType, fileName) = await userProfileService.GetResumeAsync(access.Application!.ApplicantUserId);
        return (stream, contentType, fileName, null);
    }

    public async Task<OperationResult<ProductResumeApplicationForOwnerDto>> ReviewAsync(
        Guid userId,
        ReviewProductResumeApplicationCommand command)
    {
        if (!command.Approved && string.IsNullOrWhiteSpace(command.ReviewNotes))
            return OperationResult<ProductResumeApplicationForOwnerDto>.Failure("دلیل رد درخواست الزامی است");

        var application = await context.ProductResumeApplications
            .Include(a => a.Product)
            .Include(a => a.ApplicantUser)
                .ThenInclude(u => u.Profile)
            .Include(a => a.ApplicantUser)
                .ThenInclude(u => u.EducationalBackgrounds)
            .FirstOrDefaultAsync(a => a.ProductResumeApplicationId == command.ProductResumeApplicationId);

        if (application?.Product == null)
            return OperationResult<ProductResumeApplicationForOwnerDto>.Failure("درخواست یافت نشد");

        if (!await CanReviewProductAsync(userId, application.Product))
            return OperationResult<ProductResumeApplicationForOwnerDto>.Failure("دسترسی غیرمجاز");

        if (application.Status != ProductResumeApplicationStatus.Pending)
            return OperationResult<ProductResumeApplicationForOwnerDto>.Failure("این درخواست قبلاً بررسی شده است");

        application.Status = command.Approved
            ? ProductResumeApplicationStatus.Approved
            : ProductResumeApplicationStatus.Rejected;
        application.ReviewNotes = command.ReviewNotes?.Trim() ?? string.Empty;
        application.ReviewedAt = DateTime.UtcNow.ToLocalTime();

        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return OperationResult<ProductResumeApplicationForOwnerDto>.Failure(ex.Message);
        }

        var productTitle = application.Product.Title;
        var title = command.Approved ? "تأیید درخواست رزومه" : "رد درخواست رزومه";
        var message = command.Approved
            ? $"درخواست رزومه شما برای آگهی «{productTitle}» تأیید شد."
            : $"درخواست رزومه شما برای آگهی «{productTitle}» رد شد. دلیل: {application.ReviewNotes}";
        await notificationService.CreateAsync(
            application.ApplicantUserId,
            title,
            message,
            NotificationType.General);

        return OperationResult<ProductResumeApplicationForOwnerDto>.Success(
            MapForOwner(application, productTitle));
    }

    private async Task<(ProductResumeApplication? Application, string? Error)> GetReviewableApplicationAsync(
        Guid userId,
        Guid productResumeApplicationId)
    {
        var application = await context.ProductResumeApplications
            .AsNoTracking()
            .Include(a => a.Product)
                .ThenInclude(p => p.CategoryAssignments)
                    .ThenInclude(c => c.Category)
            .FirstOrDefaultAsync(a => a.ProductResumeApplicationId == productResumeApplicationId);

        if (application?.Product == null)
            return (null, "درخواست یافت نشد");

        if (!await CanReviewProductAsync(userId, application.Product))
            return (null, "دسترسی غیرمجاز");

        return (application, null);
    }

    private async Task<Product?> LoadProductForReviewAsync(Guid productId)
        => await context.Products
            .AsNoTracking()
            .Include(p => p.CategoryAssignments)
                .ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

    private async Task<bool> CanReviewProductAsync(Guid userId, Product product)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        if (user.UserType is UserType.Administrator or UserType.Admin)
            return true;

        if (product.CreatedByUserId == userId)
            return true;

        return user.CenterProfileId.HasValue
               && product.CreatedByCenterProfileId == user.CenterProfileId
               && user.UserType is UserType.AdminLab or UserType.UserLab or UserType.Store;
    }

    private static ProductResumeApplicationForOwnerDto MapForOwner(ProductResumeApplication application, string productTitle)
    {
        var latestEducation = application.ApplicantUser?.EducationalBackgrounds
            .OrderByDescending(e => e.StartYear ?? 0)
            .ThenByDescending(e => e.EndYear ?? 0)
            .FirstOrDefault();

        return new ProductResumeApplicationForOwnerDto
        {
            ProductResumeApplicationId = application.ProductResumeApplicationId,
            ProductId = application.ProductId,
            ProductTitle = productTitle,
            ApplicantUserId = application.ApplicantUserId,
            ApplicantFirstName = application.ApplicantUser?.FirstName ?? string.Empty,
            ApplicantLastName = application.ApplicantUser?.LastName ?? string.Empty,
            ApplicantMobileNumber = application.ApplicantUser?.MobileNumber ?? string.Empty,
            JobTitle = application.ApplicantUser?.Profile?.JobTitle ?? string.Empty,
            DegreeLevel = latestEducation?.DegreeLevel,
            Status = application.Status,
            ReviewNotes = application.ReviewNotes,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
        };
    }

    private static ProductResumeApplicationDto Map(ProductResumeApplication application, string productTitle)
        => new()
        {
            ProductResumeApplicationId = application.ProductResumeApplicationId,
            ProductId = application.ProductId,
            ProductTitle = productTitle,
            Status = application.Status,
            ReviewNotes = application.ReviewNotes,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
        };
}
