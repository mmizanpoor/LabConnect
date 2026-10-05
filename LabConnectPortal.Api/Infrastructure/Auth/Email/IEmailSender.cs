namespace LabConnectPortal.Api.Infrastructure.Auth.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body);
}
