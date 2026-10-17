using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService service;
    private readonly UserManager<User> userManager;

    public NotificationController(INotificationService service, UserManager<User> userManager)
    {
        this.service = service;
        this.userManager = userManager;
    }

    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        var notifications = await service.getUserNotifications(user.Id);
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> markAsRead(int id)
    {
        await service.markAsRead(id);
        return RedirectToAction("index");
    }
}