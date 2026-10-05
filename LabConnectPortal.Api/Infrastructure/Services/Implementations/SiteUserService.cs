using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteUserService(
    IUserRepository userRepository,
    ISiteUserPermissionRepository permissionRepository,
    OtpService otpService) : ISiteUserService
{
    public async Task<OperationResult<List<SiteMemberDto>>> GetMembersAsync(Guid adminUserId)
    {
        var admin = await GetAdministratorOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<List<SiteMemberDto>>.Failure(admin.Error);

        var members = await userRepository.GetByUserTypeAsync(UserType.Admin);
        return OperationResult<List<SiteMemberDto>>.Success(members.Select(MapToDto).ToList());
    }

    public async Task<OperationResult<AddSiteMemberResultDto>> AddMemberAsync(
        Guid adminUserId,
        AddSiteMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber))
            return OperationResult<AddSiteMemberResultDto>.Failure("شماره موبایل الزامی است");

        var admin = await GetAdministratorOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<AddSiteMemberResultDto>.Failure(admin.Error);

        var mobileNumber = command.MobileNumber.Trim();

        if (await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.Admin) != null)
            return OperationResult<AddSiteMemberResultDto>.Failure("این شماره موبایل قبلاً به‌عنوان کاربر سایت ثبت شده است");

        if (await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.Administrator) != null)
            return OperationResult<AddSiteMemberResultDto>.Failure("این شماره موبایل متعلق به مدیرکل است");

        var user = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (user != null)
        {
            await otpService.SendOtpAsync(mobileNumber, OtpPurpose.AddSiteMember);
            return OperationResult<AddSiteMemberResultDto>.Success(new AddSiteMemberResultDto
            {
                RequiresOtp = true,
                OtpScenario = "user",
            });
        }

        if (await userRepository.ExistsByMobileAsync(mobileNumber))
            return OperationResult<AddSiteMemberResultDto>.Failure("این شماره موبایل قبلاً با نوع دیگری ثبت شده است");

        var newUser = CreateUser(mobileNumber);
        userRepository.Add(newUser);
        var createResult = await userRepository.SaveChangesAsync();
        if (!createResult.Success)
            return OperationResult<AddSiteMemberResultDto>.Failure(createResult.Message ?? "خطا در ثبت کاربر");

        await otpService.SendOtpAsync(mobileNumber, OtpPurpose.AddSiteMember);
        return OperationResult<AddSiteMemberResultDto>.Success(new AddSiteMemberResultDto
        {
            RequiresOtp = true,
            OtpScenario = "create",
        });
    }

    public async Task<OperationResult<SiteMemberDto>> ConfirmAddMemberAsync(
        Guid adminUserId,
        ConfirmAddSiteMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber) || string.IsNullOrWhiteSpace(command.Code))
            return OperationResult<SiteMemberDto>.Failure("شماره موبایل و کد تأیید الزامی است");

        var admin = await GetAdministratorOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<SiteMemberDto>.Failure(admin.Error);

        var mobileNumber = command.MobileNumber.Trim();

        if (await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.Admin) != null)
            return OperationResult<SiteMemberDto>.Failure("این شماره موبایل قبلاً به‌عنوان کاربر سایت ثبت شده است");

        var otp = await otpService.ValidateOtpAsync(mobileNumber, command.Code, OtpPurpose.AddSiteMember);
        if (otp == null)
            return OperationResult<SiteMemberDto>.Failure("کد وارد شده نامعتبر یا منقضی شده است");

        var user = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (user == null)
            return OperationResult<SiteMemberDto>.Failure("کاربر با این شماره موبایل یافت نشد");

        ApplyAdminMembership(user, mobileNumber);
        userRepository.Update(user);
        otpService.MarkOtpUsed(otp);
        var convertResult = await userRepository.SaveChangesAsync();
        if (!convertResult.Success)
            return OperationResult<SiteMemberDto>.Failure(convertResult.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteMemberDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult<SiteMemberDto>> ToggleMemberActiveAsync(
        Guid adminUserId,
        ToggleSiteMemberActiveCommand command)
    {
        var memberResult = await GetManagedMemberOrFailure(adminUserId, command.MemberId);
        if (memberResult.Error != null)
            return OperationResult<SiteMemberDto>.Failure(memberResult.Error);

        var member = memberResult.Member!;
        member.IsActive = command.IsActive;
        userRepository.Update(member);
        var saveResult = await userRepository.SaveChangesAsync();
        if (!saveResult.Success)
            return OperationResult<SiteMemberDto>.Failure(saveResult.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<SiteMemberDto>.Success(MapToDto(member));
    }

    public async Task<OperationResult> RemoveMemberAsync(Guid adminUserId, RemoveSiteMemberCommand command)
    {
        var memberResult = await GetManagedMemberOrFailure(adminUserId, command.MemberId);
        if (memberResult.Error != null)
            return OperationResult.Failure(memberResult.Error);

        var member = memberResult.Member!;
        ApplyUserMembership(member);
        await permissionRepository.DeleteByUserIdAsync(member.Id);
        userRepository.Update(member);
        var saveResult = await userRepository.SaveChangesAsync();
        if (!saveResult.Success)
            return OperationResult.Failure(saveResult.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult.SuccessResult();
    }

    private async Task<(User? Member, string? Error)> GetManagedMemberOrFailure(Guid adminUserId, Guid memberId)
    {
        var admin = await GetAdministratorOrFailure(adminUserId);
        if (admin.Error != null)
            return (null, admin.Error);

        if (memberId == adminUserId)
            return (null, "امکان مدیریت حساب خود وجود ندارد");

        var member = await userRepository.GetWithRolesAsync(memberId);
        if (member == null)
            return (null, "کاربر یافت نشد");

        if (member.UserType != UserType.Admin)
            return (null, "این کاربر قابل مدیریت نیست");

        return (member, null);
    }

    private static void ApplyUserMembership(User user)
    {
        user.UserType = UserType.User;
        user.Username = user.MobileNumber;
    }

    private async Task<(User? User, string? Error)> GetAdministratorOrFailure(Guid adminUserId)
    {
        var admin = await userRepository.GetWithRolesAsync(adminUserId);
        if (admin == null || admin.UserType != UserType.Administrator)
            return (null, "دسترسی مجاز نیست");

        return (admin, null);
    }

    private static void ApplyAdminMembership(User user, string mobileNumber)
    {
        user.UserType = UserType.Admin;
        user.Username = $"{mobileNumber}_{(int)UserType.Admin}";
        user.MobileConfirmed = true;
        user.CenterProfileId = null;
    }

    private static User CreateUser(string mobileNumber) => new()
    {
        Id = Guid.NewGuid(),
        UserType = UserType.User,
        Username = mobileNumber,
        MobileNumber = mobileNumber,
        PasswordHash = string.Empty,
        IsActive = true,
        MobileConfirmed = false,
        CreatedAt = DateTime.UtcNow,
    };

    private static SiteMemberDto MapToDto(User user) => new()
    {
        Id = user.Id,
        UserType = user.UserType,
        MobileNumber = user.MobileNumber,
        FirstName = user.FirstName,
        LastName = user.LastName,
        IsActive = user.IsActive,
        MobileConfirmed = user.MobileConfirmed,
        CreatedAt = user.CreatedAt,
    };
}
