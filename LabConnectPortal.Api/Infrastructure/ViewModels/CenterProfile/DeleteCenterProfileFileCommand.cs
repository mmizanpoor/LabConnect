using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class DeleteCenterProfileFileCommand
{
    public CenterProfileFileKind Kind { get; set; }
}
