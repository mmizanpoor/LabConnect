namespace LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

public class AddLabMemberResultDto
{
    public bool RequiresOtp { get; set; }
    public string? OtpScenario { get; set; }
    public LabMemberDto? Member { get; set; }
}
