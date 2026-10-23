using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class NotificationController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public NotificationController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    
    [HttpGet]
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var notifications = context.notifications
            .Where(n => n.userId == user.Id)
            .OrderByDescending(n => n.date)
            .ToList();

        return View("~/Views/Notification/Index.cshtml", notifications);
    }

   
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> markAsRead(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var notification = await context.notifications.FindAsync(id);
        if (notification == null) return NotFound();

        if (notification.userId != user.Id) return Forbid();

        notification.isRead = true;
        await context.SaveChangesAsync();

        return RedirectToAction("index");
    }
}