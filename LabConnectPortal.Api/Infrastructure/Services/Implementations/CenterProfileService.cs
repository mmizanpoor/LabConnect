using BehPardakhtProd;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Auth.Email;
using LabConnectPortal.Api.Infrastructure.Factories.PaymentFactory;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway.BehPardakht;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using LabConnectPortal.Api.Shared;
using LaboratoryApi.Models.DTO.PaymentGateway;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class CenterProfileService(
    ICenterProfileRepository centerProfileRepository,
    IUserRepository userRepository,
    ISRLabRepository srLabRepository,
    IFileStorageService fileStorageService,
    INotificationService notificationService,
    ISiteChargeServicePricingService siteChargeServicePricingService,
    OtpService otpService,
    IEmailSender emailSender,
    IPaymentFactory paymentFactory,
    IHttpContextAccessor httpContextAccessor,
    IPaymentOrderRepository paymentOrderRepository) : ICenterProfileService
{
    public async Task<OperationResult<CenterProfileDto>> GetMyProfileAsync(Guid userId)
    {
        try
        {
            var user = await userRepository.GetWithRolesAsync(userId);
            if (user == null)
                return OperationResult<CenterProfileDto>.Failure("کاربر یافت نشد");

            if (user.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
                return OperationResult<CenterProfileDto>.Failure("این بخش فقط برای آزمایشگاه و فروشگاه است");

            if ((user.UserType is UserType.AdminLab or UserType.UserLab) && !user.CenterProfileId.HasValue)
                return OperationResult<CenterProfileDto>.Failure("پروفایل مرکز برای حساب کاربری شما ثبت نشده است");

            var (profile, error) = await GetOrCreateProfileForUserAsync(user);
            if (profile == null)
                return OperationResult<CenterProfileDto>.Failure(error ?? "امکان ایجاد پروفایل وجود ندارد");

            return OperationResult<CenterProfileDto>.Success(await MapProfileAsync(profile));
        }
        catch (Exception ex)
        {
            return OperationResult<CenterProfileDto>.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }

    public async Task<OperationResult<SmsChargeInfoDto>> GetSmsChargeInfoAsync(Guid userId)
    {
        var access = await GetEditableProfileAsync(userId);
        if (access.Error != null)
            return OperationResult<SmsChargeInfoDto>.Failure(access.Error);

        var pricing = await siteChargeServicePricingService.GetPricingAsync(SiteChargeServiceCode.Sms);
        if (!pricing.Status || pricing.Data == null)
            return OperationResult<SmsChargeInfoDto>.Failure(pricing.Message ?? "تعرفه پیامک تعریف نشده است");

        decimal? unitPrice = pricing.Data.PricingMode switch
        {
            SiteChargePricingMode.Fixed => pricing.Data.Prices.FirstOrDefault()?.Price,
            SiteChargePricingMode.Range => pricing.Data.Prices.FirstOrDefault()?.Price,
            _ => null,
        };

        return OperationResult<SmsChargeInfoDto>.Success(new SmsChargeInfoDto
        {
            RemainingCount = access.Profile!.SmsCount,
            PricingMode = pricing.Data.PricingMode,
            UnitPrice = unitPrice,
            Prices = pricing.Data.Prices,
        });
    }

    public async Task<OperationResult<CenterProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateCenterProfileCommand command)
    {
        var access = await GetEditableProfileAsync(userId);
        if (access.Error != null)
            return OperationResult<CenterProfileDto>.Failure(access.Error);

        var profile = access.Profile!;
        var wasApproved = profile.IsApproved;

        var ownerId = await ResolveOwnerUserIdAsync(profile) ?? userId;
        var owner = await userRepository.GetWithRolesAsync(ownerId);
        if (owner != null && IsOwnerContactChanged(owner, command))
        {
            var otpCheck = await ValidateContactChangeOtpAsync(owner, command.Channel, command.Code);
            if (!otpCheck.Success)
                return OperationResult<CenterProfileDto>.Failure(otpCheck.Message ?? "کد تأیید نامعتبر است");
        }

        var contactError = await ApplyOwnerContactAsync(profile, userId, command);
        if (contactError != null)
            return OperationResult<CenterProfileDto>.Failure(contactError);

        profile.Name = command.Name?.Trim() ?? string.Empty;
        profile.Address = command.Address?.Trim() ?? string.Empty;
        profile.Phone = command.Phone?.Trim() ?? string.Empty;
        profile.EstablishedYear = command.EstablishedYear;
        profile.Description = command.Description?.Trim() ?? string.Empty;
        profile.Website = string.IsNullOrWhiteSpace(command.Website) ? null : command.Website.Trim();
        profile.EmployeeCount = command.EmployeeCount;
        profile.EconomicCode = string.IsNullOrWhiteSpace(command.EconomicCode) ? null : command.EconomicCode.Trim();
        profile.RegistrationNumber = string.IsNullOrWhiteSpace(command.RegistrationNumber) ? null : command.RegistrationNumber.Trim();

        RecomputeCompletion(profile);
        if (wasApproved && RequiresReapproval(profile))
            RevokeApproval(profile);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<CenterProfileDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var userSave = await userRepository.SaveChangesAsync();
        if (!userSave.Success)
            return OperationResult<CenterProfileDto>.Failure(userSave.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<CenterProfileDto>.Success(await MapProfileAsync(profile));
    }

    public async Task<OperationResult> SendContactChangeOtpAsync(Guid userId, SendCenterContactChangeOtpCommand command)
    {
        var channel = NormalizeOtpChannel(command.Channel);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید نامعتبر است");

        var access = await GetEditableProfileAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var profile = access.Profile!;
        var ownerId = await ResolveOwnerUserIdAsync(profile) ?? userId;
        var owner = await userRepository.GetWithRolesAsync(ownerId);
        if (owner == null || !owner.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(owner.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            await otpService.SendOtpAsync(owner.MobileNumber.Trim(), OtpPurpose.ChangeCenterContact);
            return OperationResult.SuccessResult();
        }

        var email = NormalizeEmail(owner.Email);
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("ایمیل برای این حساب ثبت نشده است");

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        owner.EmailConfirmationToken = BuildContactChangeEmailOtpToken(code, expiresAt);
        userRepository.Update(owner);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        try
        {
            await emailSender.SendAsync(
                email,
                "کد تأیید تغییر اطلاعات تماس LabConnect",
                BuildContactChangeEmailHtml(code));
        }
        catch (Exception)
        {
            return OperationResult.Failure("ارسال ایمیل ناموفق بود. تنظیمات SMTP را بررسی کنید.");
        }

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult<CenterProfileDto>> UploadFileAsync(
        Guid userId,
        CenterProfileFileKind kind,
        IFormFile file)
    {
        var access = await GetEditableProfileAsync(userId);
        if (access.Error != null)
            return OperationResult<CenterProfileDto>.Failure(access.Error);

        var profile = access.Profile!;
        var wasApproved = profile.IsApproved;
        var existingPath = kind switch
        {
            CenterProfileFileKind.Logo => profile.LogoPath,
            CenterProfileFileKind.NationalCard => profile.NationalCardPath,
            CenterProfileFileKind.License => profile.LicensePath,
            CenterProfileFileKind.OfficialImage => profile.OfficialImagePath,
            CenterProfileFileKind.TradeCard => profile.TradeCardPath,
            _ => null,
        };

        var saveResult = await fileStorageService.SaveCenterProfileFileAsync(profile.Id, kind, file, existingPath);
        if (!saveResult.Status || saveResult.Data == null)
            return OperationResult<CenterProfileDto>.Failure(saveResult.Message ?? "خطا در ذخیره فایل");

        switch (kind)
        {
            case CenterProfileFileKind.Logo:
                profile.LogoPath = saveResult.Data;
                break;
            case CenterProfileFileKind.NationalCard:
                profile.NationalCardPath = saveResult.Data;
                break;
            case CenterProfileFileKind.License:
                profile.LicensePath = saveResult.Data;
                break;
            case CenterProfileFileKind.OfficialImage:
                profile.OfficialImagePath = saveResult.Data;
                break;
            case CenterProfileFileKind.TradeCard:
                profile.TradeCardPath = saveResult.Data;
                break;
        }

        RecomputeCompletion(profile);
        if (wasApproved && RequiresReapproval(profile))
            RevokeApproval(profile);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<CenterProfileDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<CenterProfileDto>.Success(await MapProfileAsync(profile));
    }

    public async Task<OperationResult<CenterProfileDto>> DeleteFileAsync(
        Guid userId,
        CenterProfileFileKind kind)
    {
        var access = await GetEditableProfileAsync(userId);
        if (access.Error != null)
            return OperationResult<CenterProfileDto>.Failure(access.Error);

        var profile = access.Profile!;
        var wasApproved = profile.IsApproved;
        var existingPath = kind switch
        {
            CenterProfileFileKind.Logo => profile.LogoPath,
            CenterProfileFileKind.NationalCard => profile.NationalCardPath,
            CenterProfileFileKind.License => profile.LicensePath,
            CenterProfileFileKind.OfficialImage => profile.OfficialImagePath,
            CenterProfileFileKind.TradeCard => profile.TradeCardPath,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(existingPath))
            return OperationResult<CenterProfileDto>.Failure("فایلی برای حذف یافت نشد");

        try
        {
            fileStorageService.DeleteFileIfExists(existingPath);
        }
        catch
        {
            // Keep deleting the database path even if the physical file is already gone.
        }

        switch (kind)
        {
            case CenterProfileFileKind.Logo:
                profile.LogoPath = null;
                break;
            case CenterProfileFileKind.NationalCard:
                profile.NationalCardPath = null;
                break;
            case CenterProfileFileKind.License:
                profile.LicensePath = null;
                break;
            case CenterProfileFileKind.OfficialImage:
                profile.OfficialImagePath = null;
                break;
            case CenterProfileFileKind.TradeCard:
                profile.TradeCardPath = null;
                break;
        }

        RecomputeCompletion(profile);
        if (wasApproved && RequiresReapproval(profile))
            RevokeApproval(profile);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<CenterProfileDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<CenterProfileDto>.Success(await MapProfileAsync(profile));
    }

    public async Task<OperationResult<PagedResult<CenterProfileListItemDto>>> GetLaboratoriesAsync(GetCenterProfilesQuery query)
    {
        await EnsureLaboratoryProfilesAsync();
        var result = await centerProfileRepository.GetLaboratoriesPagedAsync(query);
        return OperationResult<PagedResult<CenterProfileListItemDto>>.Success(result);
    }

    public async Task<OperationResult<PagedResult<CenterProfileListItemDto>>> GetShopsAsync(GetCenterProfilesQuery query)
    {
        await EnsureStoreProfilesAsync();
        var result = await centerProfileRepository.GetShopsPagedAsync(query);
        return OperationResult<PagedResult<CenterProfileListItemDto>>.Success(result);
    }

    public async Task<OperationResult<CenterProfileListItemDto>> CreateLaboratoryAsync(CreateLaboratoryCommand command)
    {
        const string labExistsMessage = "آزمایشگاه وجود دارد می‌توانید شماره موبایل را ویرایش کنید";
        const string labNotFoundMessage = "آزمایشگاهی با این مشخصات یافت نشد";

        var mobile = command.MobileNumber?.Trim() ?? string.Empty;
        if (!IsValidMobileNumber(mobile))
            return OperationResult<CenterProfileListItemDto>.Failure("شماره موبایل نامعتبر است");

        if (command.LabCodeNew is < 10000 or > 99999)
            return OperationResult<CenterProfileListItemDto>.Failure("کد مرکز باید ۵ رقمی باشد");

        var legacyLab = await srLabRepository.GetSRLabName(command.LabCodeNew);
        if (legacyLab == null)
            return OperationResult<CenterProfileListItemDto>.Failure(labNotFoundMessage);

        var labCode = legacyLab.intLabId;
        var labCodeNew = command.LabCodeNew;

        if (await userRepository.GetByMobileAndUserTypeAsync(mobile, UserType.AdminLab) != null)
            return OperationResult<CenterProfileListItemDto>.Failure(labExistsMessage);

        if (await centerProfileRepository.GetByLabCodeAsync(labCode) != null ||
            await centerProfileRepository.GetByLabCodeNewAsync(labCodeNew) != null)
            return OperationResult<CenterProfileListItemDto>.Failure(labExistsMessage);

        var username = BuildAdminLabUsername(mobile);
        if (await userRepository.ExistsByUsernameAsync(username))
            return OperationResult<CenterProfileListItemDto>.Failure(labExistsMessage);

        var profile = new CenterProfile
        {
            Id = Guid.NewGuid(),
            CenterType = CenterType.Lab,
            Status = CenterProfileStatus.Active,
            LabCode = labCode,
            LabCodeNew = labCodeNew,
            Name = legacyLab.vchLabName?.Trim() ?? string.Empty,
            IsApproved = true,
            ApprovedAt = DateTime.UtcNow.ToLocalTime(),
        };
        centerProfileRepository.Add(profile);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserType = UserType.AdminLab,
            CenterProfileId = profile.Id,
            Username = username,
            MobileNumber = mobile,
            PasswordHash = string.Empty,
            IsActive = true,
            MobileConfirmed = false,
            CreatedAt = DateTime.UtcNow,
        };

        userRepository.Add(user);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<CenterProfileListItemDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<CenterProfileListItemDto>.Success(new CenterProfileListItemDto
        {
            Id = profile.Id,
            Name = profile.Name,
            LabCode = profile.LabCode,
            MobileNumber = mobile,
            Status = profile.Status,
            IsComplete = profile.IsComplete,
            IsApproved = profile.IsApproved,
            IsApiKeyEnabled = profile.IsApiKeyEnabled,
        });
    }

    public async Task<OperationResult> UpdateLaboratoryMobileAsync(UpdateLaboratoryMobileCommand command)
    {
        var mobile = command.MobileNumber?.Trim() ?? string.Empty;
        if (!IsValidMobileNumber(mobile))
            return OperationResult.Failure("شماره موبایل نامعتبر است");

        var profile = await centerProfileRepository.GetByIdWithTrackingAsync(command.ProfileId);
        if (profile == null || profile.CenterType != CenterType.Lab)
            return OperationResult.Failure("پروفایل آزمایشگاه یافت نشد");

        var members = await userRepository.GetByCenterProfileIdAsync(profile.Id);
        var currentAdmin = members.FirstOrDefault(u => u.UserType == UserType.AdminLab);

        if (currentAdmin == null && profile.LabCode.HasValue)
        {
            var adminId = await centerProfileRepository.GetAdminLabUserIdAsync(profile.LabCode.Value);
            if (adminId.HasValue)
                currentAdmin = await userRepository.GetWithRolesAsync(adminId.Value);
        }
        else if (currentAdmin != null)
        {
            currentAdmin = await userRepository.GetWithRolesAsync(currentAdmin.Id);
        }

        // CenterProfile exists but no AdminLab yet → create/assign admin for this mobile
        if (currentAdmin == null)
        {
            var createResult = await ResolveTargetAdminUserAsync(mobile, profile.Id, currentAdminId: null);
            if (createResult.Error != null)
                return OperationResult.Failure(createResult.Error);

            await ApplyAsAdminLabAsync(createResult.User!, mobile, profile.Id);
            var createSave = await userRepository.SaveChangesAsync();
            return createSave.Success
                ? OperationResult.SuccessResult()
                : OperationResult.Failure(createSave.Message ?? "خطا در ذخیره‌سازی");
        }

        if (currentAdmin.MobileNumber == mobile)
            return OperationResult.SuccessResult();

        var targetResult = await ResolveTargetAdminUserAsync(mobile, profile.Id, currentAdmin.Id);
        if (targetResult.Error != null)
            return OperationResult.Failure(targetResult.Error);

        var demoteConflict = await userRepository.GetByMobileAndUserTypeAsync(
            currentAdmin.MobileNumber, UserType.UserLab);
        if (demoteConflict != null && demoteConflict.Id != currentAdmin.Id)
            return OperationResult.Failure("امکان تغییر نقش مدیر قبلی وجود ندارد؛ کاربر آزمایشگاه با این موبایل از قبل وجود دارد");

        // Demote current AdminLab → UserLab (same center)
        currentAdmin.UserType = UserType.UserLab;
        currentAdmin.CenterProfileId = profile.Id;
        currentAdmin.Username = BuildUsername(currentAdmin.MobileNumber, UserType.UserLab);
        userRepository.Update(currentAdmin);

        await ApplyAsAdminLabAsync(targetResult.User!, mobile, profile.Id);

        var save = await userRepository.SaveChangesAsync();
        return save.Success
            ? OperationResult.SuccessResult()
            : OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");
    }

    private async Task ApplyAsAdminLabAsync(User target, string mobile, Guid centerProfileId)
    {
        if (target.Id == Guid.Empty)
        {
            var created = new User
            {
                Id = Guid.NewGuid(),
                UserType = UserType.AdminLab,
                CenterProfileId = centerProfileId,
                Username = BuildUsername(mobile, UserType.AdminLab),
                MobileNumber = mobile,
                PasswordHash = string.Empty,
                IsActive = true,
                MobileConfirmed = false,
                CreatedAt = DateTime.UtcNow,
            };
            userRepository.Add(created);
            return;
        }

        target.UserType = UserType.AdminLab;
        target.CenterProfileId = centerProfileId;
        target.Username = BuildUsername(mobile, UserType.AdminLab);
        target.MobileConfirmed = false;
        userRepository.Update(target);
    }

    private async Task<(User? User, string? Error)> ResolveTargetAdminUserAsync(
        string mobile,
        Guid centerProfileId,
        Guid? currentAdminId)
    {
        var existingAdminLab = await userRepository.GetByMobileAndUserTypeAsync(mobile, UserType.AdminLab);
        if (existingAdminLab != null)
        {
            if (currentAdminId.HasValue && existingAdminLab.Id == currentAdminId.Value)
                return (existingAdminLab, null);

            if (existingAdminLab.CenterProfileId.HasValue &&
                existingAdminLab.CenterProfileId != centerProfileId)
                return (null, "این شماره موبایل متعلق به مدیر آزمایشگاه دیگری است");

            return (existingAdminLab, null);
        }

        var existingStore = await userRepository.GetByMobileAndUserTypeAsync(mobile, UserType.Store);
        if (existingStore != null)
            return (null, "این شماره موبایل متعلق به فروشگاه است و قابل انتساب به آزمایشگاه نیست");

        // Prefer UserLab of this lab, then any UserLab, then ordinary User
        var existingUserLab = await userRepository.GetByMobileAndUserTypeAsync(mobile, UserType.UserLab);
        if (existingUserLab != null)
            return (existingUserLab, null);

        var existingUser = await userRepository.GetByMobileAndUserTypeAsync(mobile, UserType.User);
        if (existingUser != null)
            return (existingUser, null);

        // Create new AdminLab
        return (new User { Id = Guid.Empty, MobileNumber = mobile }, null);
    }

    private static string BuildUsername(string mobileNumber, UserType userType)
        => userType == UserType.User ? mobileNumber : $"{mobileNumber}_{(int)userType}";

    public async Task<OperationResult<CenterProfileDto>> GetByIdAsync(Guid id)
    {
        var profile = await centerProfileRepository.GetByIdAsync(id);
        if (profile == null)
            return OperationResult<CenterProfileDto>.Failure("پروفایل یافت نشد");

        return OperationResult<CenterProfileDto>.Success(await MapProfileAsync(profile));
    }

    public async Task<OperationResult> ApproveAsync(Guid adminUserId, ApproveCenterProfileCommand command)
    {
        var profile = await centerProfileRepository.GetByIdWithTrackingAsync(command.Id);
        if (profile == null)
            return OperationResult.Failure("پروفایل یافت نشد");

        if (profile.IsApproved)
            return OperationResult.Failure("پروفایل قبلاً تأیید شده است");

        profile.IsApproved = true;
        profile.Status = CenterProfileStatus.Active;
        profile.ApprovedAt = DateTime.UtcNow.ToLocalTime();
        profile.ApprovedByUserId = adminUserId;
        profile.RejectionReason = null;
        profile.RejectedAt = null;
        profile.RejectedByUserId = null;

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return save;

        var ownerId = await ResolveOwnerUserIdAsync(profile);
        if (ownerId.HasValue)
        {
            await notificationService.CreateAsync(
                ownerId.Value,
                "پروفایل شما تأیید شد",
                "پروفایل مرکز شما توسط مدیر سیستم تأیید شد.",
                NotificationType.Approval);
        }

        return save;
    }

    public async Task<OperationResult> EnableApiKeyAsync(ApproveCenterProfileCommand command)
    {
        var profile = await centerProfileRepository.GetByIdWithTrackingAsync(command.Id);
        if (profile == null)
            return OperationResult.Failure("پروفایل یافت نشد");

        if (profile.IsApiKeyEnabled)
            return OperationResult.SuccessResult();

        profile.IsApiKeyEnabled = true;
        return await centerProfileRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> RejectAsync(Guid adminUserId, RejectCenterProfileCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
            return OperationResult.Failure("دلیل رد الزامی است");

        var profile = await centerProfileRepository.GetByIdWithTrackingAsync(command.Id);
        if (profile == null)
            return OperationResult.Failure("پروفایل یافت نشد");

        if (!profile.IsComplete)
            return OperationResult.Failure("فقط پروفایل‌های تکمیل‌شده قابل رد هستند");

        if (profile.IsApproved)
            return OperationResult.Failure("پروفایل قبلاً تأیید شده است");

        var reason = command.Reason.Trim();
        profile.IsComplete = false;
        profile.IsApproved = false;
        profile.CompletedAt = null;
        profile.RejectionReason = reason;
        profile.RejectedAt = DateTime.UtcNow.ToLocalTime();
        profile.RejectedByUserId = adminUserId;

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return save;

        var ownerId = await ResolveOwnerUserIdAsync(profile);
        if (ownerId.HasValue)
        {
            await notificationService.CreateAsync(
                ownerId.Value,
                "پروفایل شما رد شد",
                reason,
                NotificationType.Rejection);
        }

        return save;
    }

    public async Task<OperationResult> RevokeApprovalAsync(Guid adminUserId, ApproveCenterProfileCommand command)
    {
        var profile = await centerProfileRepository.GetByIdWithTrackingAsync(command.Id);
        if (profile == null)
            return OperationResult.Failure("پروفایل یافت نشد");

        if (!profile.IsApproved)
            return OperationResult.Failure("پروفایل تأیید نشده است");

        RevokeApproval(profile);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return save;

        var ownerId = await ResolveOwnerUserIdAsync(profile);
        if (ownerId.HasValue)
        {
            await notificationService.CreateAsync(
                ownerId.Value,
                "تأیید پروفایل لغو شد",
                "تأیید پروفایل مرکز شما توسط مدیر سیستم لغو شد.",
                NotificationType.General);
        }

        return save;
    }

    public async Task<(Stream? Stream, string? ContentType, string? Error)> GetFileAsync(
        Guid requesterUserId,
        Guid profileId,
        CenterProfileFileKind kind)
    {
        var profile = await centerProfileRepository.GetByIdAsync(profileId);
        if (profile == null)
            return (null, null, "پروفایل یافت نشد");

        var user = await userRepository.GetWithRolesAsync(requesterUserId);
        if (user == null)
            return (null, null, "کاربر یافت نشد");

        // Prefer DB user type over JWT claims (roles were removed; claim may be stale/missing).
        var isSiteAdmin = user.UserType is UserType.Administrator or UserType.Admin;
        if (!isSiteAdmin && !CanUserAccessProfile(user, profile))
            return (null, null, "دسترسی مجاز نیست");

        var relativePath = kind switch
        {
            CenterProfileFileKind.Logo => profile.LogoPath,
            CenterProfileFileKind.NationalCard => profile.NationalCardPath,
            CenterProfileFileKind.License => profile.LicensePath,
            CenterProfileFileKind.OfficialImage => profile.OfficialImagePath,
            CenterProfileFileKind.TradeCard => profile.TradeCardPath,
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(relativePath))
            return (null, null, "فایل یافت نشد");

        var (stream, contentType) = await fileStorageService.OpenCenterProfileFileAsync(relativePath);
        if (stream == null)
            return (null, null, "فایل یافت نشد");

        return (stream, contentType, null);
    }

    public async Task<(Stream? Stream, string? ContentType, string? Error)> GetPublicLogoAsync(Guid profileId)
    {
        var profile = await centerProfileRepository.GetByIdAsync(profileId);
        if (profile == null || !profile.IsApproved || string.IsNullOrWhiteSpace(profile.LogoPath))
            return (null, null, "فایل یافت نشد");

        var (stream, contentType) = await fileStorageService.OpenCenterProfileFileAsync(profile.LogoPath);
        if (stream == null)
            return (null, null, "فایل یافت نشد");

        return (stream, contentType, null);
    }

    private async Task<(CenterProfile? Profile, string? Error)> GetEditableProfileAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return (null, "کاربر یافت نشد");

        if (user.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
            return (null, "این بخش فقط برای آزمایشگاه و فروشگاه است");

        if ((user.UserType is UserType.AdminLab or UserType.UserLab) && !user.CenterProfileId.HasValue)
            return (null, "پروفایل مرکز برای حساب کاربری شما ثبت نشده است");

        var (profile, error) = await GetOrCreateProfileForUserTrackedAsync(user);
        if (profile == null)
            return (null, error ?? "امکان ایجاد پروفایل وجود ندارد");

        return (profile, null);
    }

    private async Task<string?> CheckProfileWithMessage(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return ("کاربر یافت نشد");

        if (user.UserType is not (UserType.AdminLab or UserType.UserLab or UserType.Store))
            return ("این بخش فقط برای آزمایشگاه و فروشگاه است");

        if ((user.UserType is UserType.AdminLab or UserType.UserLab) && !user.CenterProfileId.HasValue)
            return ("پروفایل مرکز برای حساب کاربری شما ثبت نشده است");

        return null;
    }

    private async Task<(CenterProfile? Profile, string? Error)> GetOrCreateProfileForUserAsync(User user)
    {
        var profile = await ResolveExistingProfileAsync(user);
        if (profile != null)
            return (profile, null);

        return await CreateProfileForUserAsync(user);
    }

    private async Task<(CenterProfile? Profile, string? Error)> GetOrCreateProfileForUserTrackedAsync(User user)
    {
        var existing = await ResolveExistingProfileAsync(user);
        if (existing != null)
        {
            var tracked = await centerProfileRepository.GetByIdWithTrackingAsync(existing.Id);
            return (tracked ?? existing, tracked == null ? "پروفایل مرکز یافت نشد" : null);
        }

        return await CreateProfileForUserAsync(user);
    }

    private async Task<CenterProfile?> ResolveExistingProfileAsync(User user)
    {
        if (user.CenterProfile != null)
            return user.CenterProfile;

        if (user.CenterProfileId.HasValue)
        {
            var byId = await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);
            if (byId != null)
                return byId;
        }

        if (user.UserType != UserType.Store)
            return null;

        var byOwner = await centerProfileRepository.GetByOwnerUserIdAsync(user.Id);
        if (byOwner != null)
            return byOwner;

        return (await centerProfileRepository.GetAllAsync(p => p.OwnerUserId == user.Id))
            .OrderByDescending(p => p.CenterType == CenterType.Store)
            .FirstOrDefault();
    }

    private async Task<(CenterProfile? Profile, string? Error)> CreateProfileForUserAsync(User user)
    {
        if (user.UserType != UserType.Store)
            return (null, "امکان ایجاد پروفایل وجود ندارد");

        var profileId = user.CenterProfileId is { } existingId && existingId != Guid.Empty
            ? existingId
            : Guid.NewGuid();

        var profile = new CenterProfile
        {
            Id = profileId,
            CenterType = CenterType.Store,
            Status = CenterProfileStatus.Active,
            OwnerUserId = user.Id,
            Name = user.FirstName,
        };

        centerProfileRepository.Add(profile);
        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return (null, save.Message ?? "امکان ایجاد پروفایل وجود ندارد");

        if (user.CenterProfileId != profile.Id)
        {
            user.CenterProfileId = profile.Id;
            var userSave = await userRepository.SaveChangesAsync();
            if (!userSave.Success)
                return (null, userSave.Message ?? "امکان اتصال پروفایل به حساب کاربری وجود ندارد");
        }

        return (profile, null);
    }

    private static bool CanUserAccessProfile(User user, CenterProfile profile)
    {
        return profile.CenterType switch
        {
            CenterType.Lab => (user.UserType is UserType.AdminLab or UserType.UserLab)
                              && user.CenterProfileId == profile.Id,
            CenterType.Store => user.UserType == UserType.Store &&
                                       (user.CenterProfileId == profile.Id || profile.OwnerUserId == user.Id),
            _ => false,
        };
    }

    private static bool IsValidMobileNumber(string mobile)
        => System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^09\d{9}$");

    private static string BuildAdminLabUsername(string mobileNumber)
        => $"{mobileNumber}_{(int)UserType.AdminLab}";

    private async Task<string?> ResolveMobileNumberAsync(CenterProfile profile)
    {
        if (profile.CenterType == CenterType.Lab && profile.LabCode.HasValue)
            return await centerProfileRepository.GetAdminLabMobileAsync(profile.LabCode.Value);

        if (profile.CenterType == CenterType.Store && profile.OwnerUserId.HasValue)
        {
            var owner = await userRepository.GetWithRolesAsync(profile.OwnerUserId.Value);
            return owner?.MobileNumber;
        }

        return null;
    }

    private static void RecomputeCompletion(CenterProfile profile)
    {
        var complete =
            !string.IsNullOrWhiteSpace(profile.Name) &&
            !string.IsNullOrWhiteSpace(profile.LogoPath) &&
            !string.IsNullOrWhiteSpace(profile.Address) &&
            !string.IsNullOrWhiteSpace(profile.Phone) &&
            !string.IsNullOrWhiteSpace(profile.NationalCardPath) &&
            !string.IsNullOrWhiteSpace(profile.LicensePath) &&
            !string.IsNullOrWhiteSpace(profile.EconomicCode) &&
            !string.IsNullOrWhiteSpace(profile.RegistrationNumber);

        if (complete)
        {
            if (!profile.IsComplete)
                profile.CompletedAt = DateTime.UtcNow.ToLocalTime();
            profile.IsComplete = true;
            if (RequiresReapproval(profile))
                RevokeApproval(profile);
            profile.RejectionReason = null;
            profile.RejectedAt = null;
            profile.RejectedByUserId = null;
        }
        else
        {
            profile.IsComplete = false;
            profile.CompletedAt = null;
        }
    }

    private static bool RequiresReapproval(CenterProfile profile)
        => profile.CenterType != CenterType.Lab;

    private static void RevokeApproval(CenterProfile profile)
    {
        profile.IsApproved = false;
        profile.Status = CenterProfileStatus.Pending;
        profile.ApprovedAt = null;
        profile.ApprovedByUserId = null;
    }

    private async Task<Guid?> ResolveOwnerUserIdAsync(CenterProfile profile)
    {
        if (profile.CenterType == CenterType.Store)
            return profile.OwnerUserId;

        if (profile.CenterType != CenterType.Lab)
            return null;

        if (profile.LabCode.HasValue)
        {
            var byCode = await centerProfileRepository.GetAdminLabUserIdAsync(profile.LabCode.Value);
            if (byCode.HasValue)
                return byCode;
        }

        var members = await userRepository.GetByCenterProfileIdAsync(profile.Id);
        return members.FirstOrDefault(u => u.UserType == UserType.AdminLab)?.Id
            ?? members.FirstOrDefault()?.Id;
    }

    private Task EnsureLaboratoryProfilesAsync()
        => Task.CompletedTask;

    private async Task EnsureStoreProfilesAsync()
    {
        var stores = (await userRepository.GetAllAsync(u => u.UserType == UserType.Store)).Cast<User>().ToList();
        var needSave = false;

        foreach (var store in stores)
        {
            var existing = await centerProfileRepository.GetByOwnerUserIdAsync(store.Id);
            if (existing != null)
            {
                if (store.CenterProfileId != existing.Id)
                {
                    var tracked = await userRepository.GetWithRolesAsync(store.Id);
                    if (tracked != null)
                    {
                        tracked.CenterProfileId = existing.Id;
                        userRepository.Update(tracked);
                        needSave = true;
                    }
                }

                continue;
            }

            var profile = new CenterProfile
            {
                Id = Guid.NewGuid(),
                CenterType = CenterType.Store,
                Status = CenterProfileStatus.Active,
                OwnerUserId = store.Id,
            };
            centerProfileRepository.Add(profile);

            var trackedStore = await userRepository.GetWithRolesAsync(store.Id);
            if (trackedStore != null)
            {
                trackedStore.CenterProfileId = profile.Id;
                userRepository.Update(trackedStore);
            }

            needSave = true;
        }

        if (needSave)
            await centerProfileRepository.SaveChangesAsync();
    }

    private async Task<string?> ApplyOwnerContactAsync(
        CenterProfile profile,
        Guid currentUserId,
        UpdateCenterProfileCommand command)
    {
        var ownerId = await ResolveOwnerUserIdAsync(profile) ?? currentUserId;
        var owner = await userRepository.GetWithRolesAsync(ownerId);
        if (owner == null)
            return null;

        var mobile = command.MobileNumber?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(mobile))
        {
            if (!IsValidMobileNumber(mobile))
                return "شماره موبایل نامعتبر است";

            if (!string.Equals(owner.MobileNumber, mobile, StringComparison.Ordinal))
            {
                var duplicate = await userRepository.GetByMobileAndUserTypeAsync(mobile, owner.UserType);
                if (duplicate != null && duplicate.Id != owner.Id)
                    return "این شماره موبایل قبلاً ثبت شده است";

                var oldUsername = BuildUsername(owner.MobileNumber, owner.UserType);
                if (string.Equals(owner.Username, oldUsername, StringComparison.Ordinal))
                {
                    var newUsername = BuildUsername(mobile, owner.UserType);
                    var usernameTaken = await userRepository.GetByUsernameAsync(newUsername);
                    if (usernameTaken != null && usernameTaken.Id != owner.Id)
                        return "این نام کاربری قبلاً ثبت شده است";

                    owner.Username = newUsername;
                }

                owner.MobileNumber = mobile;
                owner.MobileConfirmed = false;
            }
        }

        var email = command.Email?.Trim() ?? string.Empty;
        if (!string.Equals(owner.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailDuplicate = await userRepository.GetByEmailAsync(email);
                if (emailDuplicate != null && emailDuplicate.Id != owner.Id)
                    return "این ایمیل قبلاً توسط کاربر دیگری ثبت شده است";
            }

            owner.EmailConfirmed = false;
        }

        owner.Email = email;
        owner.Latitude = command.Latitude;
        owner.Longitude = command.Longitude;
        ClearContactChangeEmailOtp(owner);
        userRepository.Update(owner);
        return null;
    }

    private static bool IsOwnerContactChanged(User owner, UpdateCenterProfileCommand command)
    {
        var mobile = command.MobileNumber?.Trim() ?? string.Empty;
        var email = command.Email?.Trim() ?? string.Empty;

        var mobileChanged = !string.IsNullOrEmpty(mobile)
            && !string.Equals(owner.MobileNumber, mobile, StringComparison.Ordinal);
        var emailChanged = !string.Equals(owner.Email ?? string.Empty, email, StringComparison.OrdinalIgnoreCase);
        return mobileChanged || emailChanged;
    }

    private async Task<OperationResult> ValidateContactChangeOtpAsync(User owner, string? channelRaw, string? codeRaw)
    {
        var channel = NormalizeOtpChannel(channelRaw);
        var code = NormalizeOtpCode(codeRaw);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید الزامی است");
        if (code.Length != 6 || code.Any(c => c is < '0' or > '9'))
            return OperationResult.Failure("کد تأیید نامعتبر است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(owner.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            var otp = await otpService.ValidateOtpAsync(
                owner.MobileNumber.Trim(),
                code,
                OtpPurpose.ChangeCenterContact);
            if (otp == null)
                return OperationResult.Failure("کد تأیید نادرست است");

            await otpService.MarkOtpUsedAsync(otp);
            return OperationResult.SuccessResult();
        }

        if (!TryParseContactChangeEmailOtpToken(owner.EmailConfirmationToken, out var storedCode, out var expiresAt))
            return OperationResult.Failure("کد تأیید یافت نشد؛ لطفاً دوباره ارسال کنید");

        if (DateTime.UtcNow > expiresAt)
            return OperationResult.Failure("کد تأیید منقضی شده است");

        if (!string.Equals(storedCode, code, StringComparison.Ordinal))
            return OperationResult.Failure("کد تأیید نادرست است");

        ClearContactChangeEmailOtp(owner);
        return OperationResult.SuccessResult();
    }

    private static string? NormalizeOtpChannel(string? channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
            return null;

        return channel.Trim().ToLowerInvariant() switch
        {
            "email" => "Email",
            "mobile" or "phone" or "sms" => "Mobile",
            _ => null
        };
    }

    private static string NormalizeEmail(string? email)
        => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static string NormalizeOtpCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        return string.Concat(code
            .Where(c => !char.IsWhiteSpace(c) && c != '-')
            .Select(c => c switch
            {
                >= '\u06F0' and <= '\u06F9' => (char)('0' + c - '\u06F0'),
                >= '\u0660' and <= '\u0669' => (char)('0' + c - '\u0660'),
                _ => c
            }));
    }

    private static string BuildContactChangeEmailOtpToken(string code, DateTime expiresAtUtc)
        => $"cchg|{code}|{expiresAtUtc.Ticks}";

    private static bool TryParseContactChangeEmailOtpToken(string? token, out string code, out DateTime expiresAtUtc)
    {
        code = string.Empty;
        expiresAtUtc = default;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('|');
        if (parts.Length != 3 || !string.Equals(parts[0], "cchg", StringComparison.Ordinal))
            return false;

        code = NormalizeOtpCode(parts[1]);
        if (!long.TryParse(parts[2], out var ticks))
            return false;

        expiresAtUtc = new DateTime(ticks, DateTimeKind.Utc);
        return code.Length == 6;
    }

    private static void ClearContactChangeEmailOtp(User user)
    {
        if (TryParseContactChangeEmailOtpToken(user.EmailConfirmationToken, out _, out _))
            user.EmailConfirmationToken = null;
    }

    private static string BuildContactChangeEmailHtml(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code);
        return $"""
            <div dir="rtl" style="margin:0;padding:0;background:#f4f5f7;font-family:Tahoma,Arial,sans-serif">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f5f7;padding:24px 12px">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden">
                      <tr>
                        <td style="padding:20px 24px 8px;text-align:center">
                          <img src="cid:labconnect-logo" alt="LabConnect" width="160" style="max-height:72px;max-width:220px;height:auto;border:0;display:inline-block" />
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 28px 8px;text-align:center">
                          <h1 style="margin:0;font-size:18px;line-height:1.5;color:#1e293b">تأیید تغییر اطلاعات تماس</h1>
                          <p style="margin:10px 0 0;font-size:13px;line-height:1.8;color:#64748b">
                            برای تأیید تغییر موبایل یا ایمیل در LabConnect، کد زیر را وارد کنید.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:12px 28px 8px;text-align:center">
                          <div style="display:inline-block;padding:14px 28px;border-radius:8px;background:#eff6ff;border:1px solid #bfdbfe">
                            <span style="font-size:28px;letter-spacing:8px;font-weight:700;color:#1d4ed8;font-family:Consolas,monospace">{safeCode}</span>
                          </div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 28px 24px;text-align:center">
                          <p style="margin:0;font-size:12px;line-height:1.8;color:#94a3b8">
                            این کد تا <strong style="color:#64748b">۵ دقیقه</strong> معتبر است.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </div>
            """;
    }

    private async Task<CenterProfileDto> MapProfileAsync(CenterProfile profile)
    {
        var ownerId = await ResolveOwnerUserIdAsync(profile);
        User? owner = ownerId.HasValue ? await userRepository.GetWithRolesAsync(ownerId.Value) : null;
        var mobile = owner?.MobileNumber ?? await ResolveMobileNumberAsync(profile);
        return MapToDto(profile, mobile, owner);
    }

    private static CenterProfileDto MapToDto(CenterProfile profile, string? mobile, User? owner = null)
        => new()
        {
            Id = profile.Id,
            CenterType = profile.CenterType,
            Status = profile.Status,
            LabCode = profile.LabCode,
            Name = profile.Name,
            Address = profile.Address,
            Phone = profile.Phone,
            EstablishedYear = profile.EstablishedYear,
            Description = profile.Description,
            Website = profile.Website,
            EmployeeCount = profile.EmployeeCount,
            EconomicCode = profile.EconomicCode,
            RegistrationNumber = profile.RegistrationNumber,
            HasLogo = !string.IsNullOrWhiteSpace(profile.LogoPath),
            HasNationalCard = !string.IsNullOrWhiteSpace(profile.NationalCardPath),
            HasLicense = !string.IsNullOrWhiteSpace(profile.LicensePath),
            HasOfficialImage = !string.IsNullOrWhiteSpace(profile.OfficialImagePath),
            HasTradeCard = !string.IsNullOrWhiteSpace(profile.TradeCardPath),
            IsComplete = profile.IsComplete,
            IsApproved = profile.IsApproved,
            IsApiKeyEnabled = profile.IsApiKeyEnabled,
            CompletedAt = profile.CompletedAt,
            ApprovedAt = profile.ApprovedAt,
            MobileNumber = mobile,
            Email = owner?.Email,
            Latitude = owner?.Latitude,
            Longitude = owner?.Longitude,
            RejectionReason = profile.RejectionReason,
            RejectedAt = profile.RejectedAt,
            SmsCount = profile.SmsCount,
        };

    public async Task<OperationResult<InitialRequestResponseDto>> InitPay(Guid userId, InitPayChargeCommand command)
    {
        var message = await CheckProfileWithMessage(userId);
        if (message != null)
            return OperationResult<InitialRequestResponseDto>.Failure(message);

        var smsService = await siteChargeServicePricingService.GetActiveByCodeAsync(SiteChargeServiceCode.Sms);
        if (smsService == null)
            return OperationResult<InitialRequestResponseDto>.Failure("سرویس پیامک فعال نیست");

        var quote = siteChargeServicePricingService.Quote(smsService, command.Count, command.PriceId);
        if (!quote.Status || quote.Data == null)
            return OperationResult<InitialRequestResponseDto>.Failure(quote.Message ?? "محاسبه مبلغ ناموفق بود");

        var paymentService = paymentFactory.GetPaymentGatewayService(PaymentGatewayType.BehPardakht);
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return OperationResult<InitialRequestResponseDto>.Failure("امکان تشخیص آدرس درخواست وجود ندارد");

        var scheme = request.Scheme;
        var host = request.Host.Value;
        var domain = $"{scheme}://{host}";
#if DEBUG
        domain = "http://localhost:5173";
#endif

        var systemTransactionKey = UniqKey.Generate();
        var initalRequestResponse = await paymentService.InitialRequest(new InitialRequestDto()
        {
            Amount = quote.Data.PayableAmount,
            Domain = domain,
            SystemTransactionKey = systemTransactionKey
        });

        if (initalRequestResponse.IsSuccess)
        {
            var order = new PaymentOrder()
            {
                UserId = userId,
                CreditQuantity = quote.Data.CreditQuantity,
                Amount = quote.Data.PayableAmount,
                BankOrderKey = initalRequestResponse.Key,
                PaymentState = PaymentState.Pending,
                SiteChargeServiceCode = smsService.Code,
                PaymentGatewayType = PaymentGatewayType.BehPardakht,
                SystemTransactionKey = systemTransactionKey
            };
            await paymentOrderRepository.CreateOrder(order);
            await paymentOrderRepository.SaveChangesAsync();
        }
        else
            return OperationResult<InitialRequestResponseDto>.Failure(initalRequestResponse.Message ?? "");

        return OperationResult<InitialRequestResponseDto>.Success(initalRequestResponse);
    }

    public async Task<PaymentResponseDto> VerifyPayMellat(BehPardakhtCallbackResponseDto callbackResponse)
    {
        var paymentVerified = false;
        try
        {
            var order = await paymentOrderRepository.GetBySystemTransactionKeyAsync(callbackResponse.SaleOrderId);
            if (order == null)
                return new PaymentResponseDto() { IsSuccess = false, Message = "اطلاعات سفارش یافت نشد. مجددا اقدام به پرداخت نمایید" };

            var verifyResponse = await VerifyAndFinalizePaymentAsync(
                order,
                new VerifyRequestDto()
                {
                    Amount = order.Amount,
                    Key = callbackResponse.SaleReferenceId,
                    SystemTransactionKey = order.SystemTransactionKey,
                    Code = callbackResponse.ResCode
                });
            paymentVerified = verifyResponse.IsSuccess;

            if (verifyResponse.IsSuccess)
                return new PaymentResponseDto() { IsSuccess = true, RefId = verifyResponse.RefId };

            return new PaymentResponseDto(){ IsSuccess = false, Message = verifyResponse.Message };
        }
        catch (DbUpdateException)
        {
            return new PaymentResponseDto() { IsSuccess = false, Message = GetPaymentDbUpdateErrorMessage(paymentVerified) };
        }
    }

    private static string GetPaymentDbUpdateErrorMessage(bool paymentVerified) =>
    paymentVerified
        ? "خطایی در ثبت اطلاعات پرداختی بوجود آمده است. لطفا با آزمایشگاه تماس بگیرید"
        : "خطای ناشناخته ای رخ داده است. در صورت کسر وجه مبلغ به حساب شما برگشت داده میشود. مجددا تلاش نمایید";

    private async Task<VerifyResponseDto> VerifyAndFinalizePaymentAsync(
            PaymentOrder order,
            VerifyRequestDto verifyRequest)
    {
        var paymentService = paymentFactory.GetPaymentGatewayService(PaymentGatewayType.BehPardakht);

        order.PaymentState = PaymentState.Verifying;
        order.UpdatedAt = DateTime.Now;
        await SaveWithRetryAsync();

        var centerProfile = await centerProfileRepository.GetProfileByUserId(order.UserId);
        if (centerProfile == null)
            return new VerifyResponseDto() { IsSuccess = false, Message = "پروفایل مرکز یافت نشد" };

        var verifyResponse = await paymentService.VerifyPay(verifyRequest);
        if (verifyResponse.IsSuccess)
        {
            
            centerProfile.SmsCount += order.CreditQuantity;
            order.RefId = verifyResponse.RefId;
            order.PaymentState = PaymentState.Completed;
            order.UpdatedAt = DateTime.Now;
            await SaveWithRetryAsync();
            return verifyResponse;
        }

        order.HasFailed = true;
        order.Message = verifyResponse.Message;
        order.PaymentState = PaymentState.Completed;
        order.UpdatedAt = DateTime.Now;
        await SaveWithRetryAsync();
        return verifyResponse;
    }
    private async Task SaveWithRetryAsync()
    {
        const int maxRetries = 3;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await paymentOrderRepository.SaveChangesAsync();
                return;
            }
            catch (DbUpdateException) when (attempt < maxRetries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * (attempt + 1)));
            }
        }
    }
}
