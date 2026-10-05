using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.User;

public class UserDto
{
    public Guid Id { get; set; }
    public UserType UserType { get; set; }
    public Guid? CenterProfileId { get; set; }
    public int? LabCode { get; set; }
    public int? LabCodeNew { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool MobileConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
}
