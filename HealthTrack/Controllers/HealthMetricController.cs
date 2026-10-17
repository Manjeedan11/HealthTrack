using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Client")]
public class HealthMetricController : Controller
{
    private readonly IHealthMetricService service;
    private readonly UserManager<User> userManager;
    private readonly HealthTrackDbContext context;

    public HealthMetricController(IHealthMetricService service,
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

        var metrics = await service.getClientMetrics(client.clientId);
        return View(metrics);
    }

    [HttpGet]
    public IActionResult log() => View();

    [HttpPost]
    public async Task<IActionResult> log(HealthMetric metric)
    {
        if (!ModelState.IsValid) return View(metric);

        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        metric.clientId = client.clientId;
        metric.bmi = await service.calculateBMI(metric.weight, client.height);

        await service.logMetric(metric);
        return RedirectToAction("index");
    }
}