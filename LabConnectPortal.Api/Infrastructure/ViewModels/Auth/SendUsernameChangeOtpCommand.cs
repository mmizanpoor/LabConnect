namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class SendUsernameChangeOtpCommand
{
    /// <summary>Email یا Mobile</summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>نام کاربری جدید — برای کنترل تکراری بودن قبل از ارسال OTP</summary>
    public string Username { get; set; } = string.Empty;
}
