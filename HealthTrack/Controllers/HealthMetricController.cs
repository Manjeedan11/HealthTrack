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

    public HealthMetricController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    
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

    
    [HttpGet]
    public IActionResult log()
    {
        return View("~/Views/Client/HealthMetric/Log.cshtml", new HealthMetric());
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> log(HealthMetric metric, [FromQuery] string? returnView)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        
        string viewMode = (returnView ?? "").ToLowerInvariant() == "progress" ? "progress" : "measurements";

        if (!ModelState.IsValid)
        {
            return View("~/Views/Client/HealthMetric/Log.cshtml", metric);
        }

        try
        {
            
            metric.clientId = client.clientId;
            metric.date = DateTime.Now;

            
            if (client.height.HasValue && client.height.Value > 0)
            {
                float heightInMeters = client.height.Value / 100f;
                float rawBmi = metric.weight / (heightInMeters * heightInMeters);
                metric.bmi = (float)Math.Round(rawBmi, 2);
            }
            else
            {
                metric.bmi = 0;
            }

            context.healthMetrics.Add(metric);
            await context.SaveChangesAsync();

           
            if (client.trainerId.HasValue)
            {
                var trainer = await context.trainers.FindAsync(client.trainerId);
                if (trainer != null)
                {
                    context.notifications.Add(new Notification
                    {
                        userId = trainer.userId,
                        message = $"{client.name} logged a new measurement: {metric.weight:0.0} kg, BMI {metric.bmi:0.0}.",
                        date = DateTime.Now,
                        isRead = false
                    });
                    await context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"Measurement logged: {metric.weight:0.0} kg · BMI {metric.bmi:0.0}.";
            return RedirectToAction("index", new { view = viewMode });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
            return View("~/Views/Client/HealthMetric/Log.cshtml", metric);
        }
    }
}