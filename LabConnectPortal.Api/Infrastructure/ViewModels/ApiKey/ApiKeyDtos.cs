namespace LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;

public class ApiKeyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyName { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
    public bool AllowAdd { get; set; }
    public bool AllowEdit { get; set; }
    public bool AllowView { get; set; }
    public Guid CenterProfileId { get; set; }
    public string CenterName { get; set; } = string.Empty;
}

public class CreateApiKeyCommand
{
    public string Name { get; set; } = string.Empty;
    public bool AllowAdd { get; set; }
    public bool AllowEdit { get; set; }
    public bool AllowView { get; set; }
}

public class UpdateApiKeyCommand : CreateApiKeyCommand
{
    public Guid Id { get; set; }
}

public class ApiKeyAuthorizationDto
{
    public Guid ApiKeyId { get; set; }
    public Guid CenterProfileId { get; set; }
    public Guid UserId { get; set; }
}
