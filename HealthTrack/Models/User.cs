using Microsoft.AspNetCore.Identity;

public class User : IdentityUser {
    public string role { get; set; } = "Client";
    public ICollection<Notification>? notifications { get; set; }
}

