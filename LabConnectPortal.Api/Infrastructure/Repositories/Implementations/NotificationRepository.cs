using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class NotificationRepository(LabConnectDbContext context)
    : LabConnectRepository<UserNotification>(context), INotificationRepository
{
    private readonly LabConnectDbContext _context = context;

    public Task<List<UserNotification>> GetByUserIdAsync(Guid userId)
        => _context.UserNotifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public Task<int> GetUnreadCountAsync(Guid userId)
        => _context.UserNotifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public Task<UserNotification?> GetByIdForUserAsync(Guid id, Guid userId)
        => _context.UserNotifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
}
