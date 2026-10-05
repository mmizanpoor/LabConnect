namespace LabConnectPortal.Api.Infrastructure.ViewModels.KitGroup;

public class KitGroupDto
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class CreateKitGroupCommand
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateKitGroupCommand
{
    public long Id { get; set; }

    public string Title { get; set; } = string.Empty;
}
