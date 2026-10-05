using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class CompanyRegulation
{
    [Key]
    public Guid CompanyRegulationId { get; set; }

    public CompanyRegulationType Type { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
}
