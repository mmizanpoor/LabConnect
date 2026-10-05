namespace LabConnectPortal.Api.Infrastructure.Auth.Email;

/// <summary>
/// تنظیمات SMTP ایمیل — مقادیر حساس فقط در کد (نه appsettings).
/// </summary>
public sealed class EmailSettings
{
    public bool Enabled { get; init; }
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public bool EnableSsl { get; init; }
    public string Pop3Host { get; init; } = string.Empty;
    public int Pop3Port { get; init; }
    public bool UsePopBeforeSmtp { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromDisplayName { get; init; } = string.Empty;
    public int ConnectTimeoutSeconds { get; init; }
    public int MessageTimeoutSeconds { get; init; }
    public int ConnectRetry { get; init; }
    public bool AsHtml { get; init; }
    public string PublicLogoUrl { get; init; } = string.Empty;
    public string PublicApiBaseUrl { get; init; } = string.Empty;

    /// <summary>تنظیمات ثابت ارسال ایمیل LabConnect — قابل خواندن از appsettings نیست.</summary>
    public static EmailSettings CreateHardcoded()
    {
        // رمز به صورت base64 نگه‌داری می‌شود تا در جستجوی سادهٔ متن دیده نشود.
        var password = Decode("bzcxWnY5OSNk");

        return new EmailSettings
        {
            Enabled = true,
            Host = "mail.ptnmed.com",
            Port = 587,
            EnableSsl = false,
            Pop3Host = "mail.ptnmed.com",
            Pop3Port = 110,
            UsePopBeforeSmtp = true,
            UserName = "LabConnect@ptnmed.com",
            Password = password,
            FromAddress = "LabConnect@ptnmed.com",
            FromDisplayName = "LabConnect",
            ConnectTimeoutSeconds = 60,
            MessageTimeoutSeconds = 100,
            ConnectRetry = 3,
            AsHtml = true,
            PublicLogoUrl = string.Empty,
            PublicApiBaseUrl = string.Empty,
        };
    }

    private static string Decode(string base64)
        => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
}
