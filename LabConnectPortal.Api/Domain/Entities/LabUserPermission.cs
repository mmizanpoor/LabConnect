using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class LabUserPermission
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid SystemEntityId { get; set; }

    public bool CanView { get; set; }

    public bool CanCreate { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }

    public virtual User User { get; set; } = null!;
}
