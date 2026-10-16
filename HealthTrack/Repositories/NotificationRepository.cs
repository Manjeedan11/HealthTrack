using Microsoft.EntityFrameworkCore;

public class NotificationRepository : INotificationRepository
{
    private readonly HealthTrackDbContext context;

    public NotificationRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Notification>> getByUser(string userId)
        => await context.notifications
            .Where(n => n.userId == userId)
            .OrderByDescending(n => n.date)
            .ToListAsync();

    public async Task add(Notification notification)
    {
        context.notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task markAsRead(int id)
    {
        var notification = await context.notifications.FindAsync(id);
        if (notification != null)
        {
            notification.isRead = true;
            await context.SaveChangesAsync();
        }
    }
}