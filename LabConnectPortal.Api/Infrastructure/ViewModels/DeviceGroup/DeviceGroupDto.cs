namespace LabConnectPortal.Api.Infrastructure.ViewModels.DeviceGroup;

public class DeviceGroupDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class CreateDeviceGroupCommand
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateDeviceGroupCommand
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
}
