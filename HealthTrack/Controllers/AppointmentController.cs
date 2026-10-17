using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class AppointmentController : Controller
{
    private readonly IAppointmentService service;
    private readonly UserManager<User> userManager;
    private readonly HealthTrackDbContext context;

    public AppointmentController(IAppointmentService service,
                                 UserManager<User> userManager,
                                 HealthTrackDbContext context)
    {
        this.service = service;
        this.userManager = userManager;
        this.context = context;
    }

    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var appointments = await service.getClientAppointments(client.clientId);
        return View(appointments);
    }

    [HttpGet]
    public IActionResult book() => View();

    [HttpPost]
    public async Task<IActionResult> book(Appointment appointment)
    {
        if (!ModelState.IsValid) return View(appointment);

        var success = await service.bookAppointment(appointment);
        if (!success)
        {
            ModelState.AddModelError("", "Could not book appointment. Check date and trainer availability.");
            return View(appointment);
        }
        return RedirectToAction("index");
    }
}