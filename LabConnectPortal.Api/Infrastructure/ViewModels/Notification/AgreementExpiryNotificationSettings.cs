namespace LabConnectPortal.Api.Infrastructure.ViewModels.Notification;

public class AgreementExpiryNotificationSettings
{
    public bool Enabled { get; set; } = true;
    public int RunAtHour { get; set; } = 8;
    public int RunAtMinute { get; set; } = 0;
    public string TimeZoneId { get; set; } = "Iran Standard Time";
}
