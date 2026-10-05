using System.Text.RegularExpressions;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LabConnectPortal.Api.Infrastructure.Auth.Email;

public class EmailSender(
    EmailSettings settings,
    ISiteSettingsService siteSettingsService,
    IWebHostEnvironment environment,
    ILogger<EmailSender> logger) : IEmailSender
{
    public const string LogoCidPlaceholder = "labconnect-logo";
    private const string LogoContentId = "logo@labconnect.local";

    private readonly EmailSettings _settings = settings;

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        if (!_settings.Enabled)
        {
            logger.LogWarning(
                "ارسال ایمیل غیرفعال است (Email:Enabled=false). به {Email} | {Subject} | {Body}",
                toEmail,
                subject,
                body);
            return;
        }

        var host = NormalizeHost(_settings.Host);
        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(_settings.FromAddress)
            || host.Contains("example.com", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "تنظیمات SMTP ناقص است. Host={Host}, From={From}. به {Email} | {Subject} | {Body}",
                _settings.Host,
                _settings.FromAddress,
                toEmail,
                subject,
                body);
            return;
        }

        var message = await BuildMessageAsync(toEmail, subject, body);
        var connectTimeoutMs = Math.Max(10, _settings.ConnectTimeoutSeconds) * 1000;
        var messageTimeoutMs = Math.Max(10, _settings.MessageTimeoutSeconds) * 1000;
        var retries = Math.Max(1, _settings.ConnectRetry);
        Exception? lastError = null;

        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                if (_settings.UsePopBeforeSmtp && !string.IsNullOrWhiteSpace(_settings.Pop3Host))
                    await AuthenticatePop3Async(connectTimeoutMs);

                await SendSmtpAsync(host, message, connectTimeoutMs, messageTimeoutMs);

                logger.LogInformation(
                    "ایمیل با موفقیت به {Email} ارسال شد (attempt {Attempt})",
                    toEmail,
                    attempt);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex;
                logger.LogWarning(
                    ex,
                    "تلاش {Attempt}/{Retries} ارسال ایمیل به {Email} ناموفق بود",
                    attempt,
                    retries,
                    toEmail);

                if (attempt < retries)
                    await Task.Delay(1000 * attempt);
            }
        }

        logger.LogError(
            lastError,
            "خطا در ارسال ایمیل به {Email} via {Host}:{Port}",
            toEmail,
            host,
            _settings.Port);
        throw new InvalidOperationException("ارسال ایمیل ناموفق بود", lastError);
    }

    private async Task<MimeMessage> BuildMessageAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            string.IsNullOrWhiteSpace(_settings.FromDisplayName) ? "LabConnect" : _settings.FromDisplayName,
            _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        if (!_settings.AsHtml)
        {
            message.Body = new TextPart("plain") { Text = StripHtml(body) };
            return message;
        }

        var builder = new BodyBuilder();
        var logo = await LoadEmailSafeLogoAsync();

        var html = body.Contains('<', StringComparison.Ordinal)
            ? body
            : $"<div style=\"font-family:Tahoma,sans-serif;font-size:14px;line-height:1.8;white-space:pre-wrap\">{System.Net.WebUtility.HtmlEncode(body).Replace("\n", "<br/>")}</div>";

        html = ApplyLogoToHtml(html, builder, logo);
        builder.HtmlBody = html;
        builder.TextBody = StripHtml(body);
        message.Body = builder.ToMessageBody();
        return message;
    }

    private string ApplyLogoToHtml(string html, BodyBuilder builder, LogoPayload? logo)
    {
        if (logo == null)
        {
            logger.LogWarning("لوگوی سازگار با ایمیل یافت نشد؛ تگ تصویر حذف می‌شود");
            return RemoveBrokenLogoTags(html);
        }

        var fileName = logo.MediaType.Contains("jpeg", StringComparison.OrdinalIgnoreCase)
            ? "logo.jpg"
            : logo.MediaType.Contains("gif", StringComparison.OrdinalIgnoreCase)
                ? "logo.gif"
                : "logo.png";

        var resource = builder.LinkedResources.Add(fileName, logo.Bytes, ContentType.Parse(logo.MediaType));
        resource.ContentId = LogoContentId;
        if (resource is MimePart part)
        {
            part.ContentDisposition = new ContentDisposition(ContentDisposition.Inline)
            {
                FileName = fileName,
            };
            part.ContentTransferEncoding = ContentEncoding.Base64;
            part.ContentType.Name = fileName;
        }

        // فقط مقدار cid را عوض کن؛ نه کل URL را با پیشوند تکراری
        html = Regex.Replace(
            html,
            $@"src=(['""])cid:{Regex.Escape(LogoCidPlaceholder)}\1",
            $"src=$1cid:{LogoContentId}$1",
            RegexOptions.IgnoreCase);

        if (!html.Contains($"cid:{LogoContentId}", StringComparison.OrdinalIgnoreCase))
        {
            html =
                $"<div style=\"text-align:center;margin:0 0 16px\"><img src=\"cid:{LogoContentId}\" alt=\"LabConnect\" width=\"160\" style=\"max-height:72px;max-width:220px;height:auto;border:0;display:inline-block\"/></div>"
                + html;
        }

        return html;
    }

    private static string RemoveBrokenLogoTags(string html)
    {
        html = Regex.Replace(
            html,
            @"<img\b[^>]*cid:labconnect-logo[^>]*>",
            string.Empty,
            RegexOptions.IgnoreCase);
        return html.Replace($"cid:{LogoCidPlaceholder}", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<LogoPayload?> LoadEmailSafeLogoAsync()
    {
        // 1) لوگوی اصلی سایت (اگر PNG/JPEG/GIF باشد)
        var primary = await LoadFromSiteAsync(() => siteSettingsService.OpenLogoAsync());
        if (primary != null && IsEmailSafeImage(primary.MediaType))
            return primary;

        if (primary != null)
            logger.LogWarning(
                "لوگوی اصلی سایت فرمت {MediaType} دارد و در ایمیل پشتیبانی نمی‌شود؛ از جایگزین استفاده می‌شود",
                primary.MediaType);

        // 2) لوگوی فوتر سایت
        var footer = await LoadFromSiteAsync(() => siteSettingsService.OpenFooterLogoAsync());
        if (footer != null && IsEmailSafeImage(footer.MediaType))
            return footer;

        // 3) فایل‌های PNG شناخته‌شده روی دیسک
        foreach (var relative in new[]
                 {
                     Path.Combine("uploads", "site", "footer-logo.png"),
                     Path.Combine("uploads", "site", "logo.png"),
                     Path.Combine("uploads", "site", "logo.jpg"),
                 })
        {
            var fromDisk = TryLoadFromDisk(relative);
            if (fromDisk != null)
                return fromDisk;
        }

        return null;
    }

    private async Task<LogoPayload?> LoadFromSiteAsync(
        Func<Task<(Stream? Stream, string? ContentType)>> open)
    {
        try
        {
            var (stream, contentType) = await open();
            if (stream == null)
                return null;

            await using (stream)
            {
                await using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToArray();
                if (bytes.Length == 0)
                    return null;

                return new LogoPayload(bytes, DetectImageMediaType(bytes, contentType));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "بارگذاری لوگو برای ایمیل ناموفق بود");
            return null;
        }
    }

    private LogoPayload? TryLoadFromDisk(string relativePath)
    {
        try
        {
            var absolute = Path.Combine(environment.ContentRootPath, relativePath);
            if (!File.Exists(absolute))
                return null;

            var bytes = File.ReadAllBytes(absolute);
            if (bytes.Length == 0)
                return null;

            var mediaType = DetectImageMediaType(bytes, null);
            return IsEmailSafeImage(mediaType) ? new LogoPayload(bytes, mediaType) : null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "خواندن فایل لوگو از دیسک ناموفق بود: {Path}", relativePath);
            return null;
        }
    }

    private static bool IsEmailSafeImage(string mediaType)
        => mediaType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
           || mediaType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)
           || mediaType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)
           || mediaType.Equals("image/gif", StringComparison.OrdinalIgnoreCase);

    private static string DetectImageMediaType(byte[] bytes, string? declaredContentType)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 8
            && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        if (bytes.Length >= 6
            && bytes[0] == (byte)'G' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F')
            return "image/gif";
        if (bytes.Length >= 12
            && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
            && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P')
            return "image/webp";

        var head = System.Text.Encoding.ASCII.GetString(bytes.AsSpan(0, Math.Min(bytes.Length, 256)));
        if (head.Contains("<svg", StringComparison.OrdinalIgnoreCase))
            return "image/svg+xml";

        if (!string.IsNullOrWhiteSpace(declaredContentType)
            && declaredContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return declaredContentType.Split(';', 2)[0].Trim();

        return "application/octet-stream";
    }

    private static string StripHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('<', StringComparison.Ordinal))
            return value;

        return Regex.Replace(value, "<[^>]+>", " ")
            .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private async Task AuthenticatePop3Async(int timeoutMs)
    {
        var popHost = NormalizeHost(_settings.Pop3Host);
        if (string.IsNullOrWhiteSpace(popHost))
            return;

        using var pop = new Pop3Client();
        pop.Timeout = timeoutMs;

        await pop.ConnectAsync(popHost, _settings.Pop3Port <= 0 ? 110 : _settings.Pop3Port, SecureSocketOptions.None);

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await pop.AuthenticateAsync(_settings.UserName, _settings.Password);

        await pop.DisconnectAsync(true);
        logger.LogInformation("POP before SMTP برای {User} روی {Host}:{Port} انجام شد",
            _settings.UserName, popHost, _settings.Pop3Port);
    }

    private async Task SendSmtpAsync(string host, MimeMessage message, int connectTimeoutMs, int messageTimeoutMs)
    {
        using var client = new SmtpClient();
        client.Timeout = Math.Max(connectTimeoutMs, messageTimeoutMs);

        var socketOptions = _settings.EnableSsl
            ? (_settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable)
            : SecureSocketOptions.None;

        await client.ConnectAsync(host, _settings.Port, socketOptions);

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
            await client.AuthenticateAsync(_settings.UserName, _settings.Password);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string NormalizeHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return string.Empty;

        var value = host.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            value = value[7..];
        else if (value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            value = value[8..];

        value = value.TrimEnd('/');
        var slash = value.IndexOf('/');
        if (slash >= 0)
            value = value[..slash];

        var colon = value.LastIndexOf(':');
        if (colon > 0 && int.TryParse(value[(colon + 1)..], out _))
            value = value[..colon];

        return value;
    }

    private sealed record LogoPayload(byte[] Bytes, string MediaType);
}
