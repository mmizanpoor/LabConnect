using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Notification;

public class ExternalNotificationSettings
{
    public string BaseUrl { get; set; } = "https://localhost:7173/";
    public string SendNotificationPath { get; set; } = "Notification/SendNotification";
    public string CustomerSearchPath { get; set; } = "Customer/Search";
}

public class NotificationCommandBase
{
    public long? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<NotificationAction> NotificationActions { get; set; } = [];
    public string? Picture { get; set; }
    public bool IsActive { get; set; } = true;
    public bool HaveToUpdate { get; set; }
    public int Priority { get; set; }
    public DateTime? ExpireDate { get; set; }
    public List<IdTitleDto> TargetUsers { get; set; } = [];
}

public class NotificationAction
{
    public NotificationActionType NotificationActionType { get; set; } = NotificationActionType.SystemEntity;
    public string Label { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Guid? SystemEntityId { get; set; } = new("7f3e21a8-4b9c-4d6e-9a1f-2c8e5d4b6a70");
    public string? Url { get; set; }
    public bool? OpenTab { get; set; }
    public string? Controller { get; set; }
    public string? Action { get; set; }
    public string? Parameter { get; set; }
}

public class IdTitleDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class CustomerDto
{
    public long Id { get; set; }
    public int Code { get; set; }
    public int BaseCode { get; set; }
    public string LabName { get; set; } = string.Empty;
}

public class SendExternalNotificationCommand
{
    public NotificationCommandBase Notification { get; set; } = new();
    public List<Guid> TargetUserIds { get; set; } = [];
}
