using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LabUserService(
    IUserRepository userRepository,
    ILabUserPermissionRepository permissionRepository,
    OtpService otpService) : ILabUserService
{
    public async Task<OperationResult<List<LabMemberDto>>> GetMembersAsync(Guid adminUserId)
    {
        var admin = await GetAdminOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<List<LabMemberDto>>.Failure(admin.Error);

        var members = await userRepository.GetByCenterProfileIdAsync(admin.User!.CenterProfileId!.Value);
        return OperationResult<List<LabMemberDto>>.Success(
            members.Select(MapToDto).ToList());
    }

    public async Task<OperationResult<AddLabMemberResultDto>> AddMemberAsync(Guid adminUserId, AddLabMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber))
            return OperationResult<AddLabMemberResultDto>.Failure("شماره موبایل الزامی است");

        var admin = await GetAdminOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<AddLabMemberResultDto>.Failure(admin.Error);

        var mobileNumber = command.MobileNumber.Trim();
        var centerProfileId = admin.User!.CenterProfileId!.Value;

        if (await userRepository.ExistsByMobileAndCenterProfileIdAsync(mobileNumber, centerProfileId))
            return OperationResult<AddLabMemberResultDto>.Failure("این شماره موبایل قبلاً در این آزمایشگاه ثبت شده است");

        var user = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (user != null)
        {
            await otpService.SendOtpAsync(mobileNumber, OtpPurpose.AddLabMember);
            return OperationResult<AddLabMemberResultDto>.Success(new AddLabMemberResultDto
            {
                RequiresOtp = true,
                OtpScenario = "user",
            });
        }

        var laboratoryUser = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.UserLab);
        if (laboratoryUser != null)
        {
            await otpService.SendOtpAsync(mobileNumber, OtpPurpose.AddLabMember);
            return OperationResult<AddLabMemberResultDto>.Success(new AddLabMemberResultDto
            {
                RequiresOtp = true,
                OtpScenario = "transfer",
            });
        }

        if (await userRepository.ExistsByMobileAsync(mobileNumber))
            return OperationResult<AddLabMemberResultDto>.Failure("این شماره موبایل قبلاً با نوع دیگری ثبت شده است");

        var newUser = CreateUser(mobileNumber);
        userRepository.Add(newUser);
        var createResult = await userRepository.SaveChangesAsync();
        if (!createResult.Success)
            return OperationResult<AddLabMemberResultDto>.Failure(createResult.Message ?? "خطا در ثبت کاربر");

        await otpService.SendOtpAsync(mobileNumber, OtpPurpose.AddLabMember);
        return OperationResult<AddLabMemberResultDto>.Success(new AddLabMemberResultDto
        {
            RequiresOtp = true,
            OtpScenario = "create",
        });
    }

    public async Task<OperationResult<LabMemberDto>> ConfirmAddMemberAsync(Guid adminUserId, ConfirmAddLabMemberCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber) || string.IsNullOrWhiteSpace(command.Code))
            return OperationResult<LabMemberDto>.Failure("شماره موبایل و کد تأیید الزامی است");

        var admin = await GetAdminOrFailure(adminUserId);
        if (admin.Error != null)
            return OperationResult<LabMemberDto>.Failure(admin.Error);

        var mobileNumber = command.MobileNumber.Trim();
        var centerProfileId = admin.User!.CenterProfileId!.Value;

        if (await userRepository.ExistsByMobileAndCenterProfileIdAsync(mobileNumber, centerProfileId))
            return OperationResult<LabMemberDto>.Failure("این شماره موبایل قبلاً در این آزمایشگاه ثبت شده است");

        var otp = await otpService.ValidateOtpAsync(mobileNumber, command.Code, OtpPurpose.AddLabMember);
        if (otp == null)
            return OperationResult<LabMemberDto>.Failure("کد وارد شده نامعتبر یا منقضی شده است");

        var user = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (user != null)
        {
            ApplyLaboratoryMembership(user, mobileNumber, centerProfileId);
            userRepository.Update(user);
            otpService.MarkOtpUsed(otp);
            var convertResult = await userRepository.SaveChangesAsync();
            if (!convertResult.Success)
                return OperationResult<LabMemberDto>.Failure(convertResult.Message ?? "خطا در ذخیره‌سازی");

            return OperationResult<LabMemberDto>.Success(MapToDto(user));
        }

        var laboratoryUser = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.UserLab);
        if (laboratoryUser != null)
        {
            laboratoryUser.CenterProfileId = centerProfileId;
            laboratoryUser.MobileConfirmed = true;
            userRepository.Update(laboratoryUser);
            otpService.MarkOtpUsed(otp);
            var transferResult = await userRepository.SaveChangesAsync();
            if (!transferResult.Success)
                return OperationResult<LabMemberDto>.Failure(transferResult.Message ?? "خطا در ذخیره‌سازی");

            return OperationResult<LabMemberDto>.Success(MapToDto(laboratoryUser));
        }

        return OperationResult<LabMemberDto>.Failure("کاربر با این شماره موبایل یافت نشد");
    }

    public async Task<OperationResult<LabMemberDto>> ToggleMemberActiveAsync(
        Guid adminUserId,
        ToggleLabMemberActiveCommand command)
    {
        var memberResult = await GetManagedMemberOrFailure(adminUserId, command.MemberId);
        if (memberResult.Error != null)
            return OperationResult<LabMemberDto>.Failure(memberResult.Error);

        var member = memberResult.Member!;
        member.IsActive = command.IsActive;
        userRepository.Update(member);
        var saveResult = await userRepository.SaveChangesAsync();
        if (!saveResult.Success)
            return OperationResult<LabMemberDto>.Failure(saveResult.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<LabMemberDto>.Success(MapToDto(member));
    }

    public async Task<OperationResult> RemoveMemberAsync(Guid adminUserId, RemoveLabMemberCommand command)
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
        var admin = await GetAdminOrFailure(adminUserId);
        if (admin.Error != null)
            return (null, admin.Error);

        if (memberId == adminUserId)
            return (null, "امکان مدیریت حساب خود وجود ندارد");

        var member = await userRepository.GetWithRolesAsync(memberId);
        if (member == null)
            return (null, "کاربر یافت نشد");

        if (member.UserType == UserType.AdminLab)
            return (null, "کاربر با نقش مدیر آزمایشگاه قابل حذف یا غیرفعال‌سازی نیست");

        if (member.UserType != UserType.UserLab)
            return (null, "این کاربر قابل مدیریت نیست");

        if (member.CenterProfileId != admin.User!.CenterProfileId)
            return (null, "کاربر متعلق به این آزمایشگاه نیست");

        return (member, null);
    }

    private static void ApplyUserMembership(User user)
    {
        user.UserType = UserType.User;
        user.CenterProfileId = null;
        user.Username = user.MobileNumber;
    }

    private async Task<(User? User, string? Error)> GetAdminOrFailure(Guid adminUserId)
    {
        var admin = await userRepository.GetWithRolesAsync(adminUserId);
        if (admin == null || admin.UserType is not (UserType.AdminLab or UserType.UserLab))
            return (null, "دسترسی مجاز نیست");

        if (!admin.CenterProfileId.HasValue)
            return (null, "پروفایل مرکز برای حساب کاربری تعریف نشده است");

        return (admin, null);
    }

    private static void ApplyLaboratoryMembership(User user, string mobileNumber, Guid centerProfileId)
    {
        user.UserType = UserType.UserLab;
        user.CenterProfileId = centerProfileId;
        user.Username = $"{mobileNumber}_{(int)UserType.UserLab}";
        user.MobileConfirmed = true;
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

    private static LabMemberDto MapToDto(User user) => new()
    {
        Id = user.Id,
        UserType = user.UserType,
        CenterProfileId = user.CenterProfileId,
        LabCode = user.GetLabCode(),
        LabCodeNew = user.GetLabCodeNew(),
        MobileNumber = user.MobileNumber,
        FirstName = user.FirstName,
        LastName = user.LastName,
        IsActive = user.IsActive,
        MobileConfirmed = user.MobileConfirmed,
        CreatedAt = user.CreatedAt,
    };
}
