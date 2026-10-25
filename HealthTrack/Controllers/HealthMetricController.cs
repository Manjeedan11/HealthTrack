using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Client")]
public class HealthMetricController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;
    private readonly IHealthMetricSyncService healthMetricSync;

    public HealthMetricController(
        HealthTrackDbContext context,
        UserManager<User> userManager,
        IHealthMetricSyncService healthMetricSync)
    {
        this.context = context;
        this.userManager = userManager;
        this.healthMetricSync = healthMetricSync;
    }

    // GET: /HealthMetric/Index
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var metrics = context.healthMetrics
            .Where(m => m.clientId == client.clientId)
            .OrderByDescending(m => m.date)
            .ToList();

        return View("~/Views/Client/HealthMetric/Index.cshtml", metrics);
    }

    // GET: /HealthMetric/Log
    [HttpGet]
    public async Task<IActionResult> log()
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var existing = await context.healthMetrics
            .FirstOrDefaultAsync(h => h.clientId == client.clientId
                                   && h.date >= todayStart
                                   && h.date < todayEnd);

        // Prefill with today's metric, or fall back to most recent metric
        if (existing == null)
        {
            var lastMetric = await context.healthMetrics
                .Where(h => h.clientId == client.clientId)
                .OrderByDescending(h => h.date)
                .FirstOrDefaultAsync();

            existing = new HealthMetric
            {
                weight = lastMetric?.weight ?? 0f,
                height = lastMetric?.height ?? client.height,
                bmi = lastMetric?.bmi ?? 0f
            };
        }

        // Read-only display values
        ViewBag.CurrentCaloriesBurned = existing.caloriesBurned;
        ViewBag.CurrentNetConsumed = existing.caloriesConsumed;

        return View("~/Views/Client/HealthMetric/Log.cshtml", existing);
    }

    // POST: /HealthMetric/Log
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> log(HealthMetric model)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        // Validate weight and height
        if (model.weight <= 0)
            ModelState.AddModelError(nameof(model.weight), "Please enter a valid weight.");

        if (!model.height.HasValue || model.height.Value <= 0)
            ModelState.AddModelError(nameof(model.height), "Please enter your height.");

        if (!ModelState.IsValid)
        {
            ViewBag.CurrentCaloriesBurned = 0;
            ViewBag.CurrentNetConsumed = 0;
            return View("~/Views/Client/HealthMetric/Log.cshtml", model);
        }

        try
        {
            // Compute BMI from submitted height + weight
            var hMeters = model.height!.Value / 100f;
            var computedBmi = (float)Math.Round(model.weight / (hMeters * hMeters), 2);

            var todayStart = DateTime.Today;
            var todayEnd = todayStart.AddDays(1);

            // Find or create today's metric
            var metric = await context.healthMetrics
                .FirstOrDefaultAsync(h => h.clientId == client.clientId
                                       && h.date >= todayStart
                                       && h.date < todayEnd);

            if (metric == null)
            {
                metric = new HealthMetric
                {
                    clientId = client.clientId,
                    date = DateTime.Now,
                    caloriesBurned = 0,
                    caloriesConsumed = 0
                };
                context.healthMetrics.Add(metric);
            }

            // Set only body composition — sync service owns calories
            metric.weight = model.weight;
            metric.height = model.height;
            metric.bmi = computedBmi;

            // Persist updated height to Client profile (fallback for future)
            if (!client.height.HasValue || client.height.Value <= 0)
                client.height = model.height;

            await context.SaveChangesAsync();

            // Recalculate calories from today's logs
            await healthMetricSync.syncTodaysCalories(client.clientId);

            TempData["Success"] = $"Measurement saved. BMI: {computedBmi}.";
            return RedirectToAction("index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
            ViewBag.CurrentCaloriesBurned = 0;
            ViewBag.CurrentNetConsumed = 0;
            return View("~/Views/Client/HealthMetric/Log.cshtml", model);
        }
    }
}