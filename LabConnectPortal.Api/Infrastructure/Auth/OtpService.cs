using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth.Sms;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

namespace LabConnectPortal.Api.Infrastructure.Auth;

public class OtpService(IOtpCodeRepository otpRepository, ISmsSender smsSender)
{
    private const int OtpLength = 6;
    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(2);

    public async Task<string> SendOtpAsync(string mobileNumber, OtpPurpose purpose)
    {
        await otpRepository.InvalidatePreviousAsync(mobileNumber, purpose);

        var code = GenerateCode();
        var otp = new OtpCode
        {
            Id = Guid.NewGuid(),
            MobileNumber = mobileNumber,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.Add(OtpTtl),
            IsUsed = false
        };

        otpRepository.Add(otp);
        var save = await otpRepository.SaveChangesAsync();
        if (!save.Success)
            throw new InvalidOperationException(save.Message ?? "خطا در ذخیره کد تأیید");

        await smsSender.SendAsync(mobileNumber, $"کد تأیید LabConnect: {code}");
        smsSender.SendSMS($"OTP : {code}", mobileNumber, null);
        return code;
    }

    public Task<OtpCode?> ValidateOtpAsync(string mobileNumber, string code, OtpPurpose purpose)
    {
        var normalizedCode = NormalizeCode(code);
        if (normalizedCode.Length != OtpLength || normalizedCode.Any(c => c is < '0' or > '9'))
            return Task.FromResult<OtpCode?>(null);

        return otpRepository.GetValidOtpAsync(mobileNumber.Trim(), normalizedCode, purpose);
    }

    public void MarkOtpUsed(OtpCode otp)
    {
        otp.IsUsed = true;
        otpRepository.Update(otp);
    }

    public async Task MarkOtpUsedAsync(OtpCode otp)
    {
        MarkOtpUsed(otp);
        await otpRepository.SaveChangesAsync();
    }

    private static string GenerateCode()
    {
        var random = Random.Shared.Next((int)Math.Pow(10, OtpLength - 1), (int)Math.Pow(10, OtpLength));
        return random.ToString();
    }

    private static string NormalizeCode(string? code)
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
}
