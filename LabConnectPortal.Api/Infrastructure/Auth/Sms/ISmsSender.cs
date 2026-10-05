namespace LabConnectPortal.Api.Infrastructure.Auth.Sms;

public interface ISmsSender
{
    Task SendAsync(string mobileNumber, string message);
    bool SendSMS(string message, string mobile, int? labCode);
}
