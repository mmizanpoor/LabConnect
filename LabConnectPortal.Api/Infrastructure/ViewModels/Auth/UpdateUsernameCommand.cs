namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class UpdateUsernameCommand
{
    public string Username { get; set; } = string.Empty;

    /// <summary>کد OTP — هنگام تغییر نام کاربری در صورت وجود ایمیل/موبایل الزامی است.</summary>
    public string? Code { get; set; }

    /// <summary>Email یا Mobile</summary>
    public string? Channel { get; set; }
}
