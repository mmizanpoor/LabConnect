using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Extensions;

public static class UserCenterProfileExtensions
{
    public static int? GetLabCode(this User? user)
        => user?.CenterProfile?.LabCode;

    public static int? GetLabCodeNew(this User? user)
        => user?.CenterProfile?.LabCodeNew;

    public static int? ResolveLaboratoryCode(this User? user)
        => user?.CenterProfile?.LabCode ?? user?.CenterProfile?.LabCodeNew;

    public static Guid? GetCenterProfileId(this User? user)
        => user?.CenterProfileId;
}
