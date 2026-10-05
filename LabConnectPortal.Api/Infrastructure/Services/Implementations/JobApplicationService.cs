using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.JobApplication;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class JobApplicationService(
    LabConnectDbContext context,
    IUserProfileRepository userProfileRepository,
    ICenterProfileRepository centerProfileRepository,
    INotificationService notificationService) : IJobApplicationService
{
    public async Task<OperationResult<JobApplicationDto>> SubmitAsync(Guid userId, SubmitJobApplicationCommand command)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<JobApplicationDto>.Failure("کاربر یافت نشد");

        if (user.UserType != UserType.User)
            return OperationResult<JobApplicationDto>.Failure("فقط کاربران عادی می‌توانند درخواست همکاری ارسال کنند");

        var posting = await context.JobPostingRequests
            .AsNoTracking()
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .FirstOrDefaultAsync(j => j.JobPostingId == command.JobPostingId);

        if (posting == null || posting.Status != JobPostingStatus.Active)
            return OperationResult<JobApplicationDto>.Failure("آگهی فعال یافت نشد");

        var userWithResume = await userProfileRepository.GetUserWithResumeAsync(userId);
        if (userWithResume == null)
            return OperationResult<JobApplicationDto>.Failure("کاربر یافت نشد");

        if (!ResumeCompletenessHelper.IsCompleteForApplication(userWithResume))
            return OperationResult<JobApplicationDto>.Failure("برای ارسال درخواست، اطلاعات پایه رزومه باید تکمیل شود");

        var existing = await context.JobApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.JobPostingId == command.JobPostingId
                && a.ApplicantUserId == userId
                && (a.Status == JobApplicationStatus.Pending || a.Status == JobApplicationStatus.Approved));

        if (existing != null)
            return OperationResult<JobApplicationDto>.Failure("درخواست فعال برای این آگهی قبلاً ثبت شده است");

        var application = new JobApplication
        {
            JobApplicationId = Guid.NewGuid(),
            JobPostingId = command.JobPostingId,
            ApplicantUserId = userId,
            Status = JobApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
        };

        context.JobApplications.Add(application);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobApplicationDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var jobTitle = BuildJobTitle(posting);
        var applicantName = $"{userWithResume.FirstName} {userWithResume.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(applicantName))
            applicantName = userWithResume.MobileNumber;

        var posterTitle = "درخواست استخدام جدید";
        var posterMessage = $"کاربر {applicantName} برای آگهی «{jobTitle}» درخواست استخدام ثبت کرد.";
        var posterUserId = posting.UserId;

        await notificationService.CreateAsync(
            posterUserId,
            posterTitle,
            posterMessage,
            NotificationType.General);

        await notificationService.SendExternalNotificationAsync(
            new NotificationCommandBase
            {
                Title = posterTitle,
                Message = posterMessage,
                IsActive = true,
                Priority = 1,
                NotificationActions =
                [
                    new NotificationAction
                    {
                            NotificationActionType = NotificationActionType.OpenUrl,
                        Label = "مشاهده درخواست‌ها",
                        Url = $"/profile/job-postings/{posting.JobPostingId}/applications",
                    },
                ],
            },
            [posterUserId]);

        var orgName = await GetOrganizationNameAsync(posting.UserId);
        return OperationResult<JobApplicationDto>.Success(MapToDto(application, userWithResume, posting, orgName));
    }

    public async Task<OperationResult<List<MyJobApplicationListItemDto>>> GetMyApplicationsAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return OperationResult<List<MyJobApplicationListItemDto>>.Failure("کاربر یافت نشد");

        if (user.UserType != UserType.User)
            return OperationResult<List<MyJobApplicationListItemDto>>.Failure("فقط کاربران عادی می‌توانند درخواست‌های خود را مشاهده کنند");

        var applications = await context.JobApplications
            .AsNoTracking()
            .Include(a => a.JobPosting)
            .ThenInclude(j => j.JobCategory)
            .Include(a => a.JobPosting)
            .ThenInclude(j => j.Location)
            .Where(a => a.ApplicantUserId == userId)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        var items = new List<MyJobApplicationListItemDto>();
        foreach (var app in applications)
        {
            var orgName = await GetOrganizationNameAsync(app.JobPosting.UserId);
            items.Add(new MyJobApplicationListItemDto
            {
                JobApplicationId = app.JobApplicationId,
                JobPostingId = app.JobPostingId,
                JobTitle = BuildJobTitle(app.JobPosting),
                OrganizationName = orgName,
                Status = app.Status,
                SubmittedAt = app.SubmittedAt,
                ReviewedAt = app.ReviewedAt,
            });
        }

        return OperationResult<List<MyJobApplicationListItemDto>>.Success(items);
    }

    public async Task<OperationResult<PagedResult<JobApplicationDto>>> GetForPostingAsync(Guid userId, GetApplicationsForPostingQuery query)
    {
        var posting = await context.JobPostingRequests
            .AsNoTracking()
            .Include(j => j.JobCategory)
            .Include(j => j.Location)
            .FirstOrDefaultAsync(j => j.JobPostingId == query.JobPostingId);

        if (posting == null)
            return OperationResult<PagedResult<JobApplicationDto>>.Failure("آگهی یافت نشد");

        if (!await CanManagePostingAsync(userId, posting.UserId))
            return OperationResult<PagedResult<JobApplicationDto>>.Failure("دسترسی غیرمجاز");

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.JobApplications
            .AsNoTracking()
            .Include(a => a.ApplicantUser)
            .Where(a => a.JobPostingId == query.JobPostingId);

        var totalCount = await dbQuery.CountAsync();
        var applications = await dbQuery
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orgName = await GetOrganizationNameAsync(posting.UserId);
        var items = applications
            .Select(a => MapToDto(a, a.ApplicantUser, posting, orgName))
            .ToList();

        return OperationResult<PagedResult<JobApplicationDto>>.Success(new PagedResult<JobApplicationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<JobApplicationDto>> ApproveAsync(Guid userId, ApproveJobApplicationCommand command)
    {
        var application = await GetApplicationForReviewAsync(command.JobApplicationId, userId);
        if (application.Error != null)
            return OperationResult<JobApplicationDto>.Failure(application.Error);

        var app = application.Application!;
        app.Status = JobApplicationStatus.Approved;
        app.ReviewedAt = DateTime.UtcNow.ToLocalTime();
        app.ReviewNotes = command.ReviewNotes?.Trim() ?? string.Empty;
        app.VisitScheduledAt = command.VisitScheduledAt;
        app.VisitLocation = command.VisitLocation?.Trim();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobApplicationDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var jobTitle = BuildJobTitle(app.JobPosting);
        var visitTime = app.VisitScheduledAt.HasValue
            ? app.VisitScheduledAt.Value.ToString("yyyy/MM/dd HH:mm")
            : "—";
        var visitLocation = string.IsNullOrWhiteSpace(app.VisitLocation) ? "—" : app.VisitLocation;
        await notificationService.CreateAsync(
            app.ApplicantUserId,
            "تأیید درخواست همکاری",
            $"درخواست شما برای {jobTitle} تأیید شد. زمان: {visitTime} مکان: {visitLocation}",
            NotificationType.General);

        var orgName = await GetOrganizationNameAsync(app.JobPosting.UserId);
        return OperationResult<JobApplicationDto>.Success(MapToDto(app, app.ApplicantUser, app.JobPosting, orgName));
    }

    public async Task<OperationResult<JobApplicationDto>> RejectAsync(Guid userId, RejectJobApplicationCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ReviewNotes))
            return OperationResult<JobApplicationDto>.Failure("توضیحات رد درخواست الزامی است");

        var application = await GetApplicationForReviewAsync(command.JobApplicationId, userId);
        if (application.Error != null)
            return OperationResult<JobApplicationDto>.Failure(application.Error);

        var app = application.Application!;
        app.Status = JobApplicationStatus.Rejected;
        app.ReviewedAt = DateTime.UtcNow.ToLocalTime();
        app.ReviewNotes = command.ReviewNotes.Trim();
        app.VisitScheduledAt = null;
        app.VisitLocation = null;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<JobApplicationDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var jobTitle = BuildJobTitle(app.JobPosting);
        await notificationService.CreateAsync(
            app.ApplicantUserId,
            "رد درخواست همکاری",
            $"درخواست شما برای {jobTitle} رد شد. توضیحات: {app.ReviewNotes}",
            NotificationType.General);

        var orgName = await GetOrganizationNameAsync(app.JobPosting.UserId);
        return OperationResult<JobApplicationDto>.Success(MapToDto(app, app.ApplicantUser, app.JobPosting, orgName));
    }

    private async Task<(JobApplication? Application, string? Error)> GetApplicationForReviewAsync(Guid jobApplicationId, Guid userId)
    {
        var application = await context.JobApplications
            .Include(a => a.ApplicantUser)
            .Include(a => a.JobPosting)
            .ThenInclude(j => j.JobCategory)
            .Include(a => a.JobPosting)
            .ThenInclude(j => j.Location)
            .FirstOrDefaultAsync(a => a.JobApplicationId == jobApplicationId);

        if (application == null)
            return (null, "درخواست یافت نشد");

        if (!await CanManagePostingAsync(userId, application.JobPosting.UserId))
            return (null, "دسترسی غیرمجاز");

        if (application.Status != JobApplicationStatus.Pending)
            return (null, "فقط درخواست‌های در انتظار بررسی قابل تأیید یا رد هستند");

        return (application, null);
    }

    private async Task<bool> CanManagePostingAsync(Guid userId, Guid posterUserId)
    {
        if (userId == posterUserId)
            return true;

        var users = await context.Users.AsNoTracking()
            .Where(user => user.Id == userId || user.Id == posterUserId)
            .Select(user => new { user.Id, user.UserType, user.CenterProfileId })
            .ToListAsync();

        var currentUser = users.FirstOrDefault(user => user.Id == userId);
        var poster = users.FirstOrDefault(user => user.Id == posterUserId);
        return currentUser?.UserType == UserType.UserLab
            && currentUser.CenterProfileId.HasValue
            && currentUser.CenterProfileId == poster?.CenterProfileId;
    }

    private async Task<string> GetOrganizationNameAsync(Guid posterUserId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == posterUserId);
        if (user == null)
            return string.Empty;

        CenterProfile? profile = null;
        if (user.CenterProfileId.HasValue)
            profile = await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);
        else if (user.UserType == UserType.Store)
            profile = await centerProfileRepository.GetByOwnerUserIdAsync(posterUserId);

        return profile?.Name ?? string.Empty;
    }

    private static string BuildJobTitle(JobPostingRequest posting)
    {
        var category = posting.JobCategory?.CategoryName ?? string.Empty;
        var location = posting.Location?.LocationName ?? string.Empty;
        if (string.IsNullOrWhiteSpace(category))
            return location;
        if (string.IsNullOrWhiteSpace(location))
            return category;
        return $"{category} - {location}";
    }

    private static JobApplicationDto MapToDto(JobApplication application, User applicant, JobPostingRequest posting, string orgName)
        => new()
        {
            JobApplicationId = application.JobApplicationId,
            JobPostingId = application.JobPostingId,
            JobTitle = BuildJobTitle(posting),
            OrganizationName = orgName,
            ApplicantFirstName = applicant.FirstName,
            ApplicantLastName = applicant.LastName,
            ApplicantMobileNumber = applicant.MobileNumber,
            Status = application.Status,
            SubmittedAt = application.SubmittedAt,
            ReviewedAt = application.ReviewedAt,
            ReviewNotes = application.ReviewNotes,
            VisitScheduledAt = application.VisitScheduledAt,
            VisitLocation = application.VisitLocation,
        };

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }
    }
}
