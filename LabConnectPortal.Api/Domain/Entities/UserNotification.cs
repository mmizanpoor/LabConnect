using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class UserNotification
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}

public class IdCodeTitle
{
    public long Id { get; set; }
    public int Code { get; set; }
    public string Title { get; set; }
}

public class ExternalCommandResult<T>
{
    public IEnumerable<string> Messages { get; set; } = [];
    public T? Data { get; set; }
}