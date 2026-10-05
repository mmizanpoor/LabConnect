using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPostingContractType
{
    [Key]
    public Guid Id { get; set; }

    public Guid JobPostingId { get; set; }

    public ContractType ContractType { get; set; }

    public virtual JobPostingRequest JobPosting { get; set; } = null!;
}
