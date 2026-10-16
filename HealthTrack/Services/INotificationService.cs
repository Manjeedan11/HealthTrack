public interface INotificationService
{
    Task send(string userId, string message);
    Task<IEnumerable<Notification>> getUserNotifications(string userId);
    Task markAsRead(int id);
}