namespace LabConnectPortal.Api.Infrastructure.ViewModels.LabAgreementSettings;

public class LabAgreementSettingsDto
{
    public Guid? Id { get; set; }
    public Guid CenterProfileId { get; set; }
    public bool UseHeaderImage { get; set; }
    public string? HeaderImagePath { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string HeaderAddress { get; set; } = string.Empty;
    public string Description1 { get; set; } = string.Empty;
    public string? HeaderLogoPath { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasHeaderImage { get; set; }
    public bool HasHeaderLogo { get; set; }
}

public class UpsertLabAgreementSettingsCommand
{
    public bool UseHeaderImage { get; set; }
    public string? LabName { get; set; }
    public string? HeaderAddress { get; set; }
    public string? Description1 { get; set; }
}
