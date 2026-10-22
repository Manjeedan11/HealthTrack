public class Notification {

    public int notificationId { get; set; }
    public string userId { get; set; }
    public User? user { get; set; }
    public string message { get; set; }
    public DateTime date { get; set; }
    public bool isRead { get; set; }
    
}