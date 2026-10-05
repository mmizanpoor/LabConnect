namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class ChangePasswordCommand
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>کد OTP در حالت فراموشی رمز</summary>
    public string? Code { get; set; }

    /// <summary>Email یا Mobile</summary>
    public string? Channel { get; set; }
}
