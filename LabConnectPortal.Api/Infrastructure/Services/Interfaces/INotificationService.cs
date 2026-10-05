using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string message, NotificationType type);
    Task<OperationResult> SendExternalNotificationAsync(
        NotificationCommandBase command,
        IEnumerable<Guid> targetUserIds,
        CancellationToken cancellationToken = default);
    Task<OperationResult> SendExternalNotificationByLabCodesAsync(
        NotificationCommandBase command,
        IEnumerable<int> labCodeNews,
        CancellationToken cancellationToken = default);
    Task<OperationResult> SendNotificationAsync(
        NotificationCommandBase command,
        CancellationToken cancellationToken = default);
    Task<OperationResult<string>> ResolveCustomerLabNameAsync(
        int labCodeNew,
        CancellationToken cancellationToken = default);
    Task<OperationResult<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId);
    Task<OperationResult<int>> GetUnreadCountAsync(Guid userId);
    Task<OperationResult> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<OperationResult<List<IdCodeTitle>>> GetActiveCustomers();
}
