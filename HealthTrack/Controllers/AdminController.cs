using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly HealthTrackDbContext context;

    public AdminController(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public IActionResult index()
    {
        ViewBag.UserCount = context.Users.Count();
        ViewBag.ClientCount = context.clients.Count();
        ViewBag.TrainerCount = context.trainers.Count();
        ViewBag.AppointmentCount = context.appointments.Count();
        return View();
    }

    public IActionResult users()
    {
        var users = context.Users.ToList();
        return View(users);
    }
}