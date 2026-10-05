using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class JobPreferenceContractType
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ContractType ContractType { get; set; }

    public virtual JobPreference JobPreference { get; set; } = null!;
}
