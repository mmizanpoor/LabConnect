using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CompanyRegulation;

public class CompanyRegulationDto
{
    public Guid CompanyRegulationId { get; set; }
    public CompanyRegulationType Type { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

public class SaveCompanyRegulationCommand
{
    public CompanyRegulationType Type { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class UpdateCompanyRegulationCommand
{
    public Guid CompanyRegulationId { get; set; }
    public string Body { get; set; } = string.Empty;
}
