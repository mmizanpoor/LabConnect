using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface INotificationRepository : IRepository<UserNotification>
{
    Task<List<UserNotification>> GetByUserIdAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<UserNotification?> GetByIdForUserAsync(Guid id, Guid userId);
}
