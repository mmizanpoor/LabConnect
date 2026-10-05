using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Auth.Email;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AuthService(
    IUserRepository userRepository,
    ICenterProfileRepository centerProfileRepository,
    ISRLabRepository srLabRepository,
    OtpService otpService,
    JwtTokenService jwtTokenService,
    ILoginAttemptService loginAttemptService,
    IEmailSender emailSender) : IAuthService
{
    public async Task<OperationResult> SendOtpAsync(SendOtpCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber))
            return OperationResult.Failure("شماره موبایل الزامی است");

        var mobileNumber = command.MobileNumber.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^09\d{9}$"))
            return OperationResult.Failure("شماره موبایل نامعتبر است");

        User? user;
        if (command.LoginUserType == UserType.User)
        {
            user = await ResolveUserForMobileLoginAsync(mobileNumber);
        }
        else if (IsLaboratoryLoginType(command.LoginUserType)
                 && command.LabCode is > 0
                 && command.LabCodeNew is > 0)
        {
            user = await ResolveOrCreateLaboratoryPortalUserAsync(
                mobileNumber,
                command.LoginUserType,
                command.LabCode.Value,
                command.LabCodeNew.Value);
        }
        else
        {
            user = await ResolveUserForOtpAsync(mobileNumber, command.LoginUserType);
        }

        if (user == null)
        {
            return command.LoginUserType != UserType.User
                ? OperationResult.Failure("حساب کاربری با این مشخصات یافت نشد")
                : OperationResult.Failure("خطا در ایجاد حساب کاربری");
        }

        if (!user.IsActive)
            return OperationResult.Failure("حساب کاربری غیرفعال است");

        try
        {
            await otpService.SendOtpAsync(mobileNumber, OtpPurpose.Login);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> SendLaboratoryRegistrationOtpAsync(SendOtpCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.MobileNumber))
            return OperationResult.Failure("شماره موبایل الزامی است");

        var mobileNumber = command.MobileNumber.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^09\d{9}$"))
            return OperationResult.Failure("شماره موبایل نامعتبر است");

        try
        {
            await otpService.SendOtpAsync(mobileNumber, OtpPurpose.RegisterLaboratory);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> RegisterLaboratoryAsync(RegisterLaboratoryCommand command)
    {
        var mobileNumber = command.MobileNumber?.Trim() ?? string.Empty;
        if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^09\d{9}$"))
            return OperationResult.Failure("شماره موبایل نامعتبر است");

        if (command.LabCode <= 0 || command.LabCodeNew <= 0)
            return OperationResult.Failure("کد آزمایشگاه نامعتبر است");

        var labName = command.LabName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(labName))
            return OperationResult.Failure("نام آزمایشگاه الزامی است");

        var otp = await otpService.ValidateOtpAsync(mobileNumber, command.Code, OtpPurpose.RegisterLaboratory);
        if (otp == null)
            return OperationResult.Failure("کد وارد شده نامعتبر یا منقضی شده است");

        var legacyLab = await srLabRepository.GetSRLabName(command.LabCodeNew);
        if (legacyLab == null)
            return OperationResult.Failure("آزمایشگاهی با این مشخصات یافت نشد");

        if (await centerProfileRepository.GetByLabCodeAsync(command.LabCode) != null ||
            await centerProfileRepository.GetByLabCodeNewAsync(command.LabCodeNew) != null)
            return OperationResult.Failure("حساب کاربری این آزمایشگاه قبلاً ثبت شده است");

        if (await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.AdminLab) != null)
            return OperationResult.Failure("این شماره موبایل قبلاً به‌عنوان مسئول آزمایشگاه ثبت شده است");

        var username = BuildUsername(mobileNumber, UserType.AdminLab);
        if (await userRepository.ExistsByUsernameAsync(username))
            return OperationResult.Failure("این شماره موبایل قبلاً به‌عنوان مسئول آزمایشگاه ثبت شده است");

        var profile = new CenterProfile
        {
            Id = Guid.NewGuid(),
            CenterType = CenterType.Lab,
            Status = CenterProfileStatus.Active,
            LabCode = legacyLab.intLabId > 0 ? legacyLab.intLabId : command.LabCode,
            LabCodeNew = command.LabCodeNew,
            Name = labName,
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
            MobileNumber = mobileNumber,
            PasswordHash = string.Empty,
            IsActive = true,
            MobileConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };
        userRepository.Add(user);

        await otpService.MarkOtpUsedAsync(otp);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult<TokenResult>> LoginApprovedLaboratoryAsync(
        LoginApprovedLaboratoryCommand command,
        string? clientIp = null,
        string? userAgent = null)
    {
        if (command.LabCodeNew <= 0)
        {
            await RecordLoginAsync(
                LoginMethod.Mobile,
                success: false,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "کد آزمایشگاه نامعتبر است");
            return OperationResult<TokenResult>.Failure("کد آزمایشگاه نامعتبر است");
        }

        var profile = await centerProfileRepository.GetByLabCodeNewAsync(command.LabCodeNew);
        if (profile == null)
        {
            await RecordLoginAsync(
                LoginMethod.Mobile,
                success: false,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "آزمایشگاه یافت نشد");
            return OperationResult<TokenResult>.Failure("آزمایشگاه یافت نشد");
        }

        return await LoginLaboratoryByCenterProfileAsync(profile.Id, clientIp, userAgent);
    }

    public async Task<OperationResult<TokenResult>> EnsurePortalLaboratoryAsync(
        EnsurePortalLaboratoryCommand command,
        string? clientIp = null,
        string? userAgent = null)
    {
        if (command.LabCode <= 0 || command.LabCodeNew <= 0)
        {
            return OperationResult<TokenResult>.Failure("کد آزمایشگاه نامعتبر است");
        }

        var existing = await centerProfileRepository.GetByLabCodeNewAsync(command.LabCodeNew)
                       ?? await centerProfileRepository.GetByLabCodeAsync(command.LabCode);
        if (existing != null)
            return await LoginLaboratoryByCenterProfileAsync(existing.Id, clientIp, userAgent);

        var legacyLab = await srLabRepository.GetSRLabName(command.LabCodeNew);
        var labName = legacyLab?.vchLabName?.Trim();
        if (string.IsNullOrWhiteSpace(labName))
            labName = $"آزمایشگاه {command.LabCodeNew}";

        var username = command.LabCodeNew.ToString();
        var password = command.LabCodeNew.ToString();

        if (await userRepository.ExistsByUsernameAsync(username))
            return OperationResult<TokenResult>.Failure("نام کاربری این آزمایشگاه قبلاً ثبت شده است");

        if (await userRepository.GetByMobileAndUserTypeAsync(username, UserType.AdminLab) != null)
            return OperationResult<TokenResult>.Failure("نام کاربری این آزمایشگاه قبلاً ثبت شده است");

        var profile = new CenterProfile
        {
            Id = Guid.NewGuid(),
            CenterType = CenterType.Lab,
            Status = CenterProfileStatus.Active,
            LabCode = command.LabCode,
            LabCodeNew = command.LabCodeNew,
            Name = labName,
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
            MobileNumber = username,
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = true,
            MobileConfirmed = false,
            CreatedAt = DateTime.UtcNow,
        };
        userRepository.Add(user);

        var save = await centerProfileRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var tracked = await userRepository.GetWithRolesAsync(user.Id);
        if (tracked == null)
            return OperationResult<TokenResult>.Failure("خطا در ایجاد حساب آزمایشگاه");

        return await IssueLaboratoryTokensAsync(tracked, clientIp, userAgent);
    }

    private async Task<OperationResult<TokenResult>> LoginLaboratoryByCenterProfileAsync(
        Guid centerProfileId,
        string? clientIp,
        string? userAgent)
    {
        var members = await userRepository.GetByCenterProfileIdAsync(centerProfileId);
        var admin = members.FirstOrDefault(u => u.UserType == UserType.AdminLab && u.IsActive)
                    ?? members.FirstOrDefault(u => u.UserType == UserType.UserLab && u.IsActive);
        if (admin == null)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "کاربر آزمایشگاه یافت نشد");
            return OperationResult<TokenResult>.Failure("کاربر آزمایشگاه یافت نشد");
        }

        var user = await userRepository.GetWithRolesAsync(admin.Id);
        if (user == null || !user.IsActive)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                userId: admin.Id,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "کاربر آزمایشگاه یافت نشد");
            return OperationResult<TokenResult>.Failure("کاربر آزمایشگاه یافت نشد");
        }

        return await IssueLaboratoryTokensAsync(user, clientIp, userAgent);
    }

    private async Task<OperationResult<TokenResult>> IssueLaboratoryTokensAsync(
        User user,
        string? clientIp,
        string? userAgent)
    {
        var tokens = jwtTokenService.GenerateTokens(user);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                userId: user.Id,
                username: user.Username,
                mobileNumber: user.MobileNumber,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: save.Message ?? "خطا در ذخیره‌سازی");
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ذخیره‌سازی");
        }

        await RecordLoginAsync(
            LoginMethod.Username,
            success: true,
            userId: user.Id,
            username: user.Username,
            mobileNumber: user.MobileNumber,
            clientIp: clientIp,
            userAgent: userAgent);
        return OperationResult<TokenResult>.Success(tokens);
    }

    private static bool IsLaboratoryLoginType(UserType userType)
        => userType is UserType.UserLab or UserType.AdminLab;

    private async Task<User?> ResolveUserForMobileLoginAsync(string mobileNumber)
    {
        var user = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (user != null)
            return user;

        var existingUser = await userRepository.GetByMobileAsync(mobileNumber);
        if (existingUser != null)
            return existingUser;

        return await CreateMinimalUserAsync(mobileNumber);
    }

    private async Task<User?> ResolveUserForLoginAsync(string mobileNumber, UserType loginUserType)
    {
        return loginUserType switch
        {
            UserType.User => await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User),
            UserType.UserLab => await userRepository.GetByMobileAndUserTypesAsync(
                mobileNumber, [UserType.UserLab, UserType.AdminLab]),
            UserType.AdminLab => await userRepository.GetByMobileAndUserTypeAsync(
                mobileNumber, UserType.AdminLab),
            UserType.Store => await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.Store),
            _ => null
        };
    }

    private async Task<User?> ResolveUserForOtpAsync(string mobileNumber, UserType loginUserType)
        => await ResolveUserForLoginAsync(mobileNumber, loginUserType)
           ?? await userRepository.GetByMobileAsync(mobileNumber);

    private static string BuildUsername(string mobileNumber, UserType userType)
        => userType == UserType.User ? mobileNumber : $"{mobileNumber}_{(int)userType}";

    private async Task<User?> CreateMinimalUserAsync(string mobileNumber)
    {
        var existingUser = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User);
        if (existingUser != null)
            return existingUser;

        if (await userRepository.ExistsByMobileAsync(mobileNumber))
            return await userRepository.GetByMobileAsync(mobileNumber);

        var username = BuildUsername(mobileNumber, UserType.User);
        if (await userRepository.ExistsByUsernameAsync(username))
        {
            return await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User)
                   ?? await userRepository.GetByMobileAsync(mobileNumber);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserType = UserType.User,
            Username = username,
            MobileNumber = mobileNumber,
            PasswordHash = string.Empty,
            IsActive = true,
            MobileConfirmed = false,
            CreatedAt = DateTime.UtcNow,
        };

        userRepository.Add(user);
        var result = await userRepository.SaveChangesAsync();
        if (!result.Success)
        {
            return await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.User)
                   ?? await userRepository.GetByMobileAsync(mobileNumber);
        }

        return user;
    }

    private async Task<User?> ResolveOrCreateLaboratoryPortalUserAsync(
        string mobileNumber,
        UserType userType,
        int labCode,
        int labCodeNew)
    {
        if (!IsLaboratoryLoginType(userType))
            return null;

        var profile = await ResolveOrCreateLaboratoryProfileAsync(labCode, labCodeNew);
        if (profile == null)
            return null;

        var existing = await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, userType);
        if (existing != null)
        {
            if (existing.CenterProfileId != profile.Id)
            {
                existing.CenterProfileId = profile.Id;
                userRepository.Update(existing);
                await userRepository.SaveChangesAsync();
            }

            return existing;
        }

        var username = BuildUsername(mobileNumber, userType);
        if (await userRepository.ExistsByUsernameAsync(username))
            return await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, userType);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserType = userType,
            CenterProfileId = profile.Id,
            Username = username,
            MobileNumber = mobileNumber,
            PasswordHash = string.Empty,
            IsActive = true,
            MobileConfirmed = false,
            CreatedAt = DateTime.UtcNow,
        };

        userRepository.Add(user);
        var result = await userRepository.SaveChangesAsync();
        if (!result.Success)
            return await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, userType);

        return user;
    }

    private async Task<CenterProfile?> ResolveOrCreateLaboratoryProfileAsync(int labCode, int labCodeNew)
    {
        var byOld = await centerProfileRepository.GetByLabCodeAsync(labCode);
        if (byOld != null)
            return byOld;

        var byNew = await centerProfileRepository.GetByLabCodeNewAsync(labCodeNew);
        if (byNew != null)
            return byNew;

        var profile = new CenterProfile
        {
            Id = Guid.NewGuid(),
            CenterType = CenterType.Lab,
            Status = CenterProfileStatus.Active,
            LabCode = labCode,
            LabCodeNew = labCodeNew,
            IsApproved = true,
            ApprovedAt = DateTime.UtcNow.ToLocalTime(),
        };
        centerProfileRepository.Add(profile);
        var save = await centerProfileRepository.SaveChangesAsync();
        return save.Success ? profile : null;
    }

    public async Task<OperationResult<TokenResult>> VerifyOtpAsync(
        VerifyOtpCommand command,
        string? clientIp = null,
        string? userAgent = null)
    {
        var mobileNumber = command.MobileNumber.Trim();
        var otp = await otpService.ValidateOtpAsync(mobileNumber, command.Code, OtpPurpose.Login);
        if (otp == null)
        {
            await RecordLoginAsync(
                LoginMethod.Mobile,
                success: false,
                mobileNumber: mobileNumber,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "کد وارد شده نامعتبر یا منقضی شده است");
            return OperationResult<TokenResult>.Failure("کد وارد شده نامعتبر یا منقضی شده است");
        }

        var user = await ResolveUserForOtpAsync(mobileNumber, command.LoginUserType);
        if (user == null || !user.IsActive)
        {
            await RecordLoginAsync(
                LoginMethod.Mobile,
                success: false,
                userId: user?.Id,
                mobileNumber: mobileNumber,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "کاربر یافت نشد یا غیرفعال است");
            return OperationResult<TokenResult>.Failure("کاربر یافت نشد یا غیرفعال است");
        }

        await otpService.MarkOtpUsedAsync(otp);

        if (!user.MobileConfirmed)
        {
            user.MobileConfirmed = true;
            userRepository.Update(user);
        }

        var tokens = jwtTokenService.GenerateTokens(user);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
        {
            await RecordLoginAsync(
                LoginMethod.Mobile,
                success: false,
                userId: user.Id,
                mobileNumber: mobileNumber,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: save.Message ?? "خطا در ذخیره‌سازی");
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ذخیره‌سازی");
        }

        await RecordLoginAsync(
            LoginMethod.Mobile,
            success: true,
            userId: user.Id,
            mobileNumber: mobileNumber,
            clientIp: clientIp,
            userAgent: userAgent);
        return OperationResult<TokenResult>.Success(tokens);
    }

    public async Task<OperationResult<TokenResult>> LoginWithPasswordAsync(
        LoginWithPasswordCommand command,
        string? clientIp = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                username: command.Username?.Trim(),
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "نام کاربری و گذرواژه الزامی است");
            return OperationResult<TokenResult>.Failure("نام کاربری و گذرواژه الزامی است");
        }

        var username = command.Username.Trim();
        var user = await userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                username: username,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "نام کاربری یا گذرواژه نامعتبر است");
            return OperationResult<TokenResult>.Failure("نام کاربری یا گذرواژه نامعتبر است");
        }

        if (!user.IsActive)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                userId: user.Id,
                username: username,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "حساب کاربری غیرفعال است");
            return OperationResult<TokenResult>.Failure("حساب کاربری غیرفعال است");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !VerifyPassword(command.Password, user.PasswordHash))
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                userId: user.Id,
                username: username,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: "نام کاربری یا گذرواژه نامعتبر است");
            return OperationResult<TokenResult>.Failure("نام کاربری یا گذرواژه نامعتبر است");
        }

        var tokens = jwtTokenService.GenerateTokens(user);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
        {
            await RecordLoginAsync(
                LoginMethod.Username,
                success: false,
                userId: user.Id,
                username: username,
                clientIp: clientIp,
                userAgent: userAgent,
                failureReason: save.Message ?? "خطا در ذخیره‌سازی");
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ذخیره‌سازی");
        }

        await RecordLoginAsync(
            LoginMethod.Username,
            success: true,
            userId: user.Id,
            username: username,
            clientIp: clientIp,
            userAgent: userAgent);
        return OperationResult<TokenResult>.Success(tokens);
    }

    private Task RecordLoginAsync(
        LoginMethod method,
        bool success,
        Guid? userId = null,
        string? username = null,
        string? mobileNumber = null,
        string? clientIp = null,
        string? userAgent = null,
        string? failureReason = null)
        => loginAttemptService.RecordAsync(new RecordLoginAttemptCommand
        {
            UserId = userId,
            LoginMethod = method,
            Success = success,
            Username = username,
            MobileNumber = mobileNumber,
            IpAddress = clientIp,
            UserAgent = userAgent,
            FailureReason = failureReason,
        });

    public async Task<OperationResult<TokenResult>> RegisterStoreAsync(RegisterStoreCommand command)
    {
        var username = command.Username.Trim();
        var mobileNumber = command.MobileNumber.Trim();
        var storeName = command.StoreName.Trim();

        if (string.IsNullOrWhiteSpace(username))
            return OperationResult<TokenResult>.Failure("نام کاربری الزامی است");

        if (username.Length > 50)
            return OperationResult<TokenResult>.Failure("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد");

        if (string.IsNullOrWhiteSpace(mobileNumber))
            return OperationResult<TokenResult>.Failure("شماره موبایل الزامی است");

        if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^09\d{9}$"))
            return OperationResult<TokenResult>.Failure("شماره موبایل نامعتبر است");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            return OperationResult<TokenResult>.Failure("رمز عبور باید حداقل ۶ کاراکتر باشد");

        if (string.IsNullOrWhiteSpace(storeName))
            return OperationResult<TokenResult>.Failure("نام فروشگاه الزامی است");

        if (storeName.Length > 200)
            return OperationResult<TokenResult>.Failure("نام فروشگاه نباید بیشتر از ۲۰۰ کاراکتر باشد");

        if (await userRepository.ExistsByUsernameAsync(username))
            return OperationResult<TokenResult>.Failure("این نام کاربری قبلاً ثبت شده است");

        if (await userRepository.GetByMobileAndUserTypeAsync(mobileNumber, UserType.Store) != null)
            return OperationResult<TokenResult>.Failure("فروشگاهی با این شماره موبایل قبلاً ثبت شده است");

        var profileId = Guid.NewGuid();
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserType = UserType.Store,
            CenterProfileId = profileId,
            Username = username,
            MobileNumber = mobileNumber,
            PasswordHash = PasswordHasher.Hash(command.Password),
            IsActive = true,
            MobileConfirmed = true,
            CreatedAt = DateTime.UtcNow,
        };

        userRepository.Add(user);

        centerProfileRepository.Add(new CenterProfile
        {
            Id = profileId,
            CenterType = CenterType.Store,
            Status = CenterProfileStatus.Active,
            OwnerUserId = user.Id,
            Name = storeName,
        });

        var tokens = jwtTokenService.GenerateTokens(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ثبت‌نام فروشگاه");

        return OperationResult<TokenResult>.Success(tokens);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return PasswordHasher.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public async Task<OperationResult<ProfileDto>> GetProfileAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult<ProfileDto>.Failure("کاربر یافت نشد");

        return OperationResult<ProfileDto>.Success(new ProfileDto
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmed,
            Address = user.Address,
            Phone = user.Phone,
            Latitude = user.Latitude,
            Longitude = user.Longitude,
            UserType = user.UserType,
            CenterProfileId = user.CenterProfileId,
            LabCode = user.GetLabCode(),
            LabCodeNew = user.GetLabCodeNew(),
        });
    }

    public async Task<OperationResult> UpdateProfileAsync(Guid userId, UpdateProfileCommand command)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        if (!string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase))
            user.EmailConfirmed = false;

        user.FirstName = command.FirstName ?? string.Empty;
        user.LastName = command.LastName ?? string.Empty;
        user.Email = command.Email ?? string.Empty;
        user.Address = command.Address ?? string.Empty;
        user.Phone = command.Phone ?? string.Empty;
        user.Latitude = command.Latitude;
        user.Longitude = command.Longitude;

        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult<TokenResult>> UpdateUsernameAsync(Guid userId, UpdateUsernameCommand command)
    {
        var username = command.Username.Trim();
        if (string.IsNullOrWhiteSpace(username))
            return OperationResult<TokenResult>.Failure("نام کاربری الزامی است");

        if (username.Length > 50)
            return OperationResult<TokenResult>.Failure("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult<TokenResult>.Failure("کاربر یافت نشد یا غیرفعال است");

        var availability = await EnsureUsernameAvailableForUserAsync(user, username);
        if (!availability.Success)
            return OperationResult<TokenResult>.Failure(availability.Message ?? "این نام کاربری قبلاً ثبت شده است");

        if (string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
        {
            var unchangedTokens = jwtTokenService.GenerateTokens(user);
            userRepository.Update(user);
            var unchangedSave = await userRepository.SaveChangesAsync();
            if (!unchangedSave.Success)
                return OperationResult<TokenResult>.Failure(unchangedSave.Message ?? "خطا در ذخیره‌سازی");

            return OperationResult<TokenResult>.Success(unchangedTokens);
        }

        var hasEmail = !string.IsNullOrWhiteSpace(user.Email);
        var hasMobile = !string.IsNullOrWhiteSpace(user.MobileNumber);
        if (hasEmail || hasMobile)
        {
            var otpCheck = await ValidateUsernameChangeOtpAsync(user, command.Channel, command.Code);
            if (!otpCheck.Success)
                return OperationResult<TokenResult>.Failure(otpCheck.Message ?? "کد تأیید نامعتبر است");
        }

        user.Username = username;
        ClearUsernameChangeEmailOtp(user);
        var tokens = jwtTokenService.GenerateTokens(user);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult<TokenResult>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return OperationResult<TokenResult>.Success(tokens);
    }

    public async Task<OperationResult> CheckUsernameAvailableAsync(Guid userId, CheckUsernameAvailableCommand command)
    {
        var username = (command.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(username))
            return OperationResult.Failure("نام کاربری الزامی است");

        if (username.Length > 50)
            return OperationResult.Failure("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        return await EnsureUsernameAvailableForUserAsync(user, username);
    }

    public async Task<OperationResult> SendUsernameChangeOtpAsync(Guid userId, SendUsernameChangeOtpCommand command)
    {
        var channel = NormalizeUsernameChangeChannel(command.Channel);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید نامعتبر است");

        var username = (command.Username ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(username))
            return OperationResult.Failure("نام کاربری الزامی است");

        if (username.Length > 50)
            return OperationResult.Failure("نام کاربری نباید بیشتر از ۵۰ کاراکتر باشد");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        var availability = await EnsureUsernameAvailableForUserAsync(user, username);
        if (!availability.Success)
            return availability;

        if (string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Failure("نام کاربری تغییری نکرده است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            await otpService.SendOtpAsync(user.MobileNumber.Trim(), OtpPurpose.ChangeUsername);
            return OperationResult.SuccessResult();
        }

        var email = NormalizeEmail(user.Email);
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("ایمیل برای این حساب ثبت نشده است");

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        user.EmailConfirmationToken = BuildUsernameChangeEmailOtpToken(code, expiresAt);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        try
        {
            await emailSender.SendAsync(
                email,
                "کد تأیید تغییر نام کاربری LabConnect",
                BuildUsernameChangeEmailHtml(code));
        }
        catch (Exception)
        {
            return OperationResult.Failure(
                "ارسال ایمیل ناموفق بود. تنظیمات SMTP را بررسی کنید.");
        }

        return OperationResult.SuccessResult();
    }

    private async Task<OperationResult> EnsureUsernameAvailableForUserAsync(User user, string username)
    {
        var duplicate = await userRepository.GetByUsernameAsync(username);
        if (duplicate != null && duplicate.Id != user.Id)
            return OperationResult.Failure("این نام کاربری قبلاً ثبت شده است");

        return OperationResult.SuccessResult();
    }

    private async Task<OperationResult> EnsureEmailAvailableForUserAsync(User user, string email)
    {
        var duplicate = await userRepository.GetByEmailAsync(email);
        if (duplicate != null && duplicate.Id != user.Id)
            return OperationResult.Failure("این ایمیل قبلاً توسط کاربر دیگری ثبت شده است");

        return OperationResult.SuccessResult();
    }

    private async Task<OperationResult> ValidateUsernameChangeOtpAsync(User user, string? channelRaw, string? codeRaw)
    {
        var channel = NormalizeUsernameChangeChannel(channelRaw);
        var code = NormalizeOtpCode(codeRaw);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید الزامی است");
        if (code.Length != 6 || code.Any(c => c is < '0' or > '9'))
            return OperationResult.Failure("کد تأیید نامعتبر است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            var otp = await otpService.ValidateOtpAsync(
                user.MobileNumber.Trim(),
                code,
                OtpPurpose.ChangeUsername);
            if (otp == null)
                return OperationResult.Failure("کد تأیید نادرست است");

            await otpService.MarkOtpUsedAsync(otp);
            return OperationResult.SuccessResult();
        }

        if (!TryParseUsernameChangeEmailOtpToken(user.EmailConfirmationToken, out var storedCode, out var expiresAt))
            return OperationResult.Failure("کد تأیید یافت نشد؛ لطفاً دوباره ارسال کنید");

        if (DateTime.UtcNow > expiresAt)
            return OperationResult.Failure("کد تأیید منقضی شده است");

        if (!string.Equals(storedCode, code, StringComparison.Ordinal))
            return OperationResult.Failure("کد تأیید نادرست است");

        ClearUsernameChangeEmailOtp(user);
        return OperationResult.SuccessResult();
    }

    private static string? NormalizeUsernameChangeChannel(string? channel)
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

    private static string BuildUsernameChangeEmailOtpToken(string code, DateTime expiresAtUtc)
        => $"uchg|{code}|{expiresAtUtc.Ticks}";

    private static bool TryParseUsernameChangeEmailOtpToken(string? token, out string code, out DateTime expiresAtUtc)
    {
        code = string.Empty;
        expiresAtUtc = default;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('|');
        if (parts.Length != 3 || !string.Equals(parts[0], "uchg", StringComparison.Ordinal))
            return false;

        code = NormalizeOtpCode(parts[1]);
        if (!long.TryParse(parts[2], out var ticks))
            return false;

        expiresAtUtc = new DateTime(ticks, DateTimeKind.Utc);
        return code.Length == 6;
    }

    private static void ClearUsernameChangeEmailOtp(User user)
    {
        if (TryParseUsernameChangeEmailOtpToken(user.EmailConfirmationToken, out _, out _))
            user.EmailConfirmationToken = null;
    }

    private static string BuildUsernameChangeEmailHtml(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code);
        return $"""
            <div dir="rtl" style="margin:0;padding:0;background:#f4f5f7;font-family:Tahoma,Arial,sans-serif">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f5f7;padding:24px 12px">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden">
                      <tr>
                        <td style="padding:20px 24px 8px;text-align:center;background:linear-gradient(180deg,#f8fafc 0%,#ffffff 100%)">
                          <img src="cid:labconnect-logo" alt="LabConnect" width="160" style="max-height:72px;max-width:220px;height:auto;border:0;display:inline-block" />
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 28px 8px;text-align:center">
                          <h1 style="margin:0;font-size:18px;line-height:1.5;color:#1e293b">تأیید تغییر نام کاربری</h1>
                          <p style="margin:10px 0 0;font-size:13px;line-height:1.8;color:#64748b">
                            برای تأیید تغییر نام کاربری در LabConnect، کد زیر را در سامانه وارد کنید.
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
                            اگر این درخواست از سمت شما نبوده، این پیام را نادیده بگیرید.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 24px;background:#f8fafc;border-top:1px solid #e2e8f0;text-align:center">
                          <p style="margin:0;font-size:11px;color:#94a3b8">LabConnect · پرتال آزمایشگاهی</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </div>
            """;
    }

    public async Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 6)
            return OperationResult.Failure("رمز عبور جدید باید حداقل ۶ کاراکتر باشد");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        var useOtp = !string.IsNullOrWhiteSpace(command.Code) || !string.IsNullOrWhiteSpace(command.Channel);
        if (useOtp)
        {
            var otpCheck = await ValidatePasswordChangeOtpAsync(user, command.Channel, command.Code);
            if (!otpCheck.Success)
                return otpCheck;
        }
        else if (!string.IsNullOrWhiteSpace(user.PasswordHash) &&
                 !VerifyPassword(command.CurrentPassword, user.PasswordHash))
        {
            return OperationResult.Failure("رمز عبور فعلی نامعتبر است");
        }

        user.PasswordHash = PasswordHasher.Hash(command.NewPassword);
        ClearPasswordChangeEmailOtp(user);
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> SendPasswordChangeOtpAsync(Guid userId, SendPasswordChangeOtpCommand command)
    {
        var channel = NormalizeUsernameChangeChannel(command.Channel);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید نامعتبر است");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            await otpService.SendOtpAsync(user.MobileNumber.Trim(), OtpPurpose.ChangePassword);
            return OperationResult.SuccessResult();
        }

        var email = NormalizeEmail(user.Email);
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("ایمیل برای این حساب ثبت نشده است");

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        user.EmailConfirmationToken = BuildPasswordChangeEmailOtpToken(code, expiresAt);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        try
        {
            await emailSender.SendAsync(
                email,
                "کد تأیید تغییر رمز عبور LabConnect",
                BuildPasswordChangeEmailHtml(code));
        }
        catch (Exception)
        {
            return OperationResult.Failure("ارسال ایمیل ناموفق بود. تنظیمات SMTP را بررسی کنید.");
        }

        return OperationResult.SuccessResult();
    }

    private async Task<OperationResult> ValidatePasswordChangeOtpAsync(User user, string? channelRaw, string? codeRaw)
    {
        var channel = NormalizeUsernameChangeChannel(channelRaw);
        var code = NormalizeOtpCode(codeRaw);
        if (channel is null)
            return OperationResult.Failure("کانال تأیید الزامی است");
        if (code.Length != 6 || code.Any(c => c is < '0' or > '9'))
            return OperationResult.Failure("کد تأیید نامعتبر است");

        if (channel == "Mobile")
        {
            if (string.IsNullOrWhiteSpace(user.MobileNumber))
                return OperationResult.Failure("شماره موبایل برای این حساب ثبت نشده است");

            var otp = await otpService.ValidateOtpAsync(
                user.MobileNumber.Trim(),
                code,
                OtpPurpose.ChangePassword);
            if (otp == null)
                return OperationResult.Failure("کد تأیید نادرست است");

            await otpService.MarkOtpUsedAsync(otp);
            return OperationResult.SuccessResult();
        }

        if (!TryParsePasswordChangeEmailOtpToken(user.EmailConfirmationToken, out var storedCode, out var expiresAt))
            return OperationResult.Failure("کد تأیید یافت نشد؛ لطفاً دوباره ارسال کنید");

        if (DateTime.UtcNow > expiresAt)
            return OperationResult.Failure("کد تأیید منقضی شده است");

        if (!string.Equals(storedCode, code, StringComparison.Ordinal))
            return OperationResult.Failure("کد تأیید نادرست است");

        ClearPasswordChangeEmailOtp(user);
        return OperationResult.SuccessResult();
    }

    private static string BuildPasswordChangeEmailOtpToken(string code, DateTime expiresAtUtc)
        => $"pchng|{code}|{expiresAtUtc.Ticks}";

    private static bool TryParsePasswordChangeEmailOtpToken(string? token, out string code, out DateTime expiresAtUtc)
    {
        code = string.Empty;
        expiresAtUtc = default;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('|');
        if (parts.Length != 3 || !string.Equals(parts[0], "pchng", StringComparison.Ordinal))
            return false;

        code = NormalizeOtpCode(parts[1]);
        if (!long.TryParse(parts[2], out var ticks))
            return false;

        expiresAtUtc = new DateTime(ticks, DateTimeKind.Utc);
        return code.Length == 6;
    }

    private static void ClearPasswordChangeEmailOtp(User user)
    {
        if (TryParsePasswordChangeEmailOtpToken(user.EmailConfirmationToken, out _, out _))
            user.EmailConfirmationToken = null;
    }

    private static string BuildPasswordChangeEmailHtml(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code);
        return $"""
            <div dir="rtl" style="margin:0;padding:0;background:#f4f5f7;font-family:Tahoma,Arial,sans-serif">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f5f7;padding:24px 12px">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden">
                      <tr>
                        <td style="padding:20px 24px 8px;text-align:center;background:linear-gradient(180deg,#f8fafc 0%,#ffffff 100%)">
                          <img src="cid:labconnect-logo" alt="LabConnect" width="160" style="max-height:72px;max-width:220px;height:auto;border:0;display:inline-block" />
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 28px 8px;text-align:center">
                          <h1 style="margin:0;font-size:18px;line-height:1.5;color:#1e293b">تأیید تغییر رمز عبور</h1>
                          <p style="margin:10px 0 0;font-size:13px;line-height:1.8;color:#64748b">
                            برای تأیید تغییر رمز عبور در LabConnect، کد زیر را در سامانه وارد کنید.
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
                            اگر این درخواست از سمت شما نبوده، این پیام را نادیده بگیرید.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 24px;background:#f8fafc;border-top:1px solid #e2e8f0;text-align:center">
                          <p style="margin:0;font-size:11px;color:#94a3b8">LabConnect · پرتال آزمایشگاهی</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </div>
            """;
    }

    public async Task<OperationResult<TokenResult>> RefreshAsync(RefreshTokenCommand command)
    {
        var user = (await userRepository.GetAllAsync(u =>
            u.RefreshToken == command.RefreshToken &&
            u.RefreshTokenExpiryTime > DateTime.UtcNow)).Cast<User>().FirstOrDefault();

        if (user == null)
            return OperationResult<TokenResult>.Failure("توکن رفرش نامعتبر است");

        var fullUser = await userRepository.GetWithRolesAsync(user.Id);
        if (fullUser == null || !fullUser.IsActive)
            return OperationResult<TokenResult>.Failure("کاربر یافت نشد");

        var tokens = jwtTokenService.GenerateTokens(fullUser);
        userRepository.Update(fullUser);
        await userRepository.SaveChangesAsync();
        return OperationResult<TokenResult>.Success(tokens);
    }

    public async Task<OperationResult> LogoutAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult.SuccessResult();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> ForgotPasswordAsync(SendOtpCommand command)
    {
        var user = await userRepository.GetByMobileAsync(command.MobileNumber);
        if (user == null)
            return OperationResult.Failure("کاربری با این شماره یافت نشد");

        await otpService.SendOtpAsync(command.MobileNumber, OtpPurpose.ResetPassword);
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordCommand command)
    {
        var otp = await otpService.ValidateOtpAsync(command.MobileNumber, command.Code, OtpPurpose.ResetPassword);
        if (otp == null)
            return OperationResult.Failure("کد نامعتبر است");

        var user = await userRepository.GetByMobileAsync(command.MobileNumber);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        await otpService.MarkOtpUsedAsync(otp);
        user.PasswordHash = PasswordHasher.Hash(command.NewPassword);
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> SendEmailConfirmationAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return OperationResult.Failure("ایمیل کاربر یافت نشد");

        user.EmailConfirmationToken ??= Guid.NewGuid().ToString("N");
        userRepository.Update(user);
        await userRepository.SaveChangesAsync();
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> ConfirmEmailAsync(string token)
    {
        var user = (await userRepository.GetAllAsync(u => u.EmailConfirmationToken == token)).Cast<User>().FirstOrDefault();
        if (user == null)
            return OperationResult.Failure("توکن نامعتبر است");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> SendEmailOtpAsync(Guid userId, SendEmailOtpCommand command)
    {
        var email = NormalizeEmail(command.Email);
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("ایمیل الزامی است");

        if (!IsValidEmail(email))
            return OperationResult.Failure("فرمت ایمیل نامعتبر است");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        var emailAvailability = await EnsureEmailAvailableForUserAsync(user, email);
        if (!emailAvailability.Success)
            return emailAvailability;

        var code = Random.Shared.Next(100000, 1000000).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        user.Email = email;
        user.EmailConfirmed = false;
        user.EmailConfirmationToken = BuildEmailOtpToken(code, expiresAt);
        userRepository.Update(user);
        var save = await userRepository.SaveChangesAsync();
        if (!save.Success)
            return OperationResult.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        try
        {
            await emailSender.SendAsync(
                email,
                "کد تأیید ایمیل LabConnect",
                BuildEmailConfirmationHtml(code));
        }
        catch (Exception)
        {
            return OperationResult.Failure(
                "ارسال ایمیل ناموفق بود. تنظیمات SMTP یا رمز عبور برنامه (App Password) را بررسی کنید.");
        }

        return OperationResult.SuccessResult();
    }

    private static string BuildEmailConfirmationHtml(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code);
        return $"""
            <div dir="rtl" style="margin:0;padding:0;background:#f4f5f7;font-family:Tahoma,Arial,sans-serif">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f5f7;padding:24px 12px">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden">
                      <tr>
                        <td style="padding:20px 24px 8px;text-align:center;background:linear-gradient(180deg,#f8fafc 0%,#ffffff 100%)">
                          <img src="cid:labconnect-logo" alt="LabConnect" width="160" style="max-height:72px;max-width:220px;height:auto;border:0;display:inline-block" />
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:8px 28px 8px;text-align:center">
                          <h1 style="margin:0;font-size:18px;line-height:1.5;color:#1e293b">تأیید ایمیل حساب کاربری</h1>
                          <p style="margin:10px 0 0;font-size:13px;line-height:1.8;color:#64748b">
                            برای تأیید آدرس ایمیل خود در LabConnect، کد زیر را در سامانه وارد کنید.
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
                            اگر این درخواست از سمت شما نبوده، این پیام را نادیده بگیرید.
                          </p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:14px 24px;background:#f8fafc;border-top:1px solid #e2e8f0;text-align:center">
                          <p style="margin:0;font-size:11px;color:#94a3b8">LabConnect · پرتال آزمایشگاهی</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </div>
            """;
    }

    public async Task<OperationResult> ConfirmEmailOtpAsync(Guid userId, ConfirmEmailOtpCommand command)
    {
        var email = NormalizeEmail(command.Email);
        var code = NormalizeOtpCode(command.Code);
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure("ایمیل الزامی است");
        if (code.Length != 6 || code.Any(c => c is < '0' or > '9'))
            return OperationResult.Failure("کد تأیید نامعتبر است");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || !user.IsActive)
            return OperationResult.Failure("کاربر یافت نشد یا غیرفعال است");

        if (!string.Equals(NormalizeEmail(user.Email), email, StringComparison.Ordinal))
            return OperationResult.Failure("ایمیل با درخواست ارسال کد مطابقت ندارد");

        var emailAvailability = await EnsureEmailAvailableForUserAsync(user, email);
        if (!emailAvailability.Success)
            return emailAvailability;

        if (!TryParseEmailOtpToken(user.EmailConfirmationToken, out var storedCode, out var expiresAt))
            return OperationResult.Failure("کد تأیید یافت نشد؛ لطفاً دوباره ارسال کنید");

        if (DateTime.UtcNow > expiresAt)
            return OperationResult.Failure("کد تأیید منقضی شده است");

        if (!string.Equals(storedCode, code, StringComparison.Ordinal))
            return OperationResult.Failure("کد تأیید نادرست است");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    private static string NormalizeEmail(string? email)
        => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string BuildEmailOtpToken(string code, DateTime expiresAtUtc)
        => $"{code}|{expiresAtUtc.Ticks}";

    private static bool TryParseEmailOtpToken(string? token, out string code, out DateTime expiresAtUtc)
    {
        code = string.Empty;
        expiresAtUtc = default;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('|', 2);
        if (parts.Length != 2)
            return false;

        code = NormalizeOtpCode(parts[0]);
        if (!long.TryParse(parts[1], out var ticks))
            return false;

        expiresAtUtc = new DateTime(ticks, DateTimeKind.Utc);
        return code.Length == 6;
    }

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

    public async Task<OperationResult> SendMobileConfirmationAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        await otpService.SendOtpAsync(user.MobileNumber, OtpPurpose.ConfirmMobile);
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> ConfirmMobileAsync(ConfirmMobileCommand command)
    {
        var otp = await otpService.ValidateOtpAsync(command.MobileNumber, command.Code, OtpPurpose.ConfirmMobile);
        if (otp == null)
            return OperationResult.Failure("کد نامعتبر است");

        var user = await userRepository.GetByMobileAsync(command.MobileNumber);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        await otpService.MarkOtpUsedAsync(otp);
        user.MobileConfirmed = true;
        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }
}
