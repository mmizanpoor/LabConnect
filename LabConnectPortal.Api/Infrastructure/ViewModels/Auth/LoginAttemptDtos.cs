using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class UserLoginLogDto
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string? DisplayName { get; set; }
    public LoginMethod LoginMethod { get; set; }
    public bool Success { get; set; }
    public string? Username { get; set; }
    public string? MobileNumber { get; set; }
    public string? IpAddress { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetLoginAttemptsQuery
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool? Success { get; set; }
    public LoginMethod? LoginMethod { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RecordLoginAttemptCommand
{
    public Guid? UserId { get; set; }
    public LoginMethod LoginMethod { get; set; }
    public bool Success { get; set; }
    public string? Username { get; set; }
    public string? MobileNumber { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }
}
