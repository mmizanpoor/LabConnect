namespace LabConnectPortal.Api.Infrastructure.ViewModels.Security;

public class SecuritySmsWalletExportDto
{
    public int intLabCode { get; set; }
    public int intLabCodeNew { get; set; }
    public string? vchLockData { get; set; }
    public string chrSenderType { get; set; } = string.Empty;
    public string chrExpDate { get; set; } = string.Empty;
    public int intSmsCount { get; set; }
    public long Wallet { get; set; }
}
