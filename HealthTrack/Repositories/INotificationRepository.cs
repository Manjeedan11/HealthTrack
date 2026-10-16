public interface INotificationRepository
{
    Task<IEnumerable<Notification>> getByUser(string userId);
    Task add(Notification notification);
    Task markAsRead(int id);
}