using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Trainer")]
public class TrainerController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public TrainerController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        var clients = context.clients.ToList();
        ViewBag.Clients = clients;
        return View(trainer);
    }
}