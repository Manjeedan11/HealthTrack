public class NotificationService : INotificationService
{
    private readonly INotificationRepository notificationRepo;

    public NotificationService(INotificationRepository notificationRepo)
    {
        this.notificationRepo = notificationRepo;
    }

    public async Task send(string userId, string message)
    {
        await notificationRepo.add(new Notification
        {
            userId = userId,
            message = message,
            date = DateTime.Now,
            isRead = false
        });
    }

    public async Task<IEnumerable<Notification>> getUserNotifications(string userId)
        => await notificationRepo.getByUser(userId);

    public async Task markAsRead(int id)
        => await notificationRepo.markAsRead(id);
}