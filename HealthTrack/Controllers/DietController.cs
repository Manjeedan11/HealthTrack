using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels.Trainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class DietController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public DietController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        List<DietPlan> plans;

        if (User.IsInRole("Trainer"))
        {
            var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
            if (trainer == null) return NotFound();

            plans = context.dietPlans
                .Include(p => p.meals)
                .Where(p => p.trainerId == trainer.trainerId)
                .ToList();
        }
        else
        {
            var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
            if (client == null) return NotFound();

            plans = context.dietPlans
                .Include(p => p.meals)
                .Where(p => p.clientId == client.clientId)
                .ToList();

            // Recent meal logs for the client
            ViewBag.RecentLogs = context.mealLogs
                .Include(m => m.meal)
                .Where(m => m.clientId == client.clientId)
                .OrderByDescending(m => m.date)
                .Take(10)
                .ToList();
        }

        return View("~/Views/Client/Diet/Index.cshtml", plans);
    }

    
    [HttpGet]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> create()
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var viewModel = new CreateDietPlanViewModel
        {
            AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList()
        };

        return View("~/Views/Trainer/Diet/Create.cshtml", viewModel);
    }

    
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> create(CreateDietPlanViewModel viewModel)
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        if (!ModelState.IsValid)
        {
            viewModel.AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList();
            return View("~/Views/Trainer/Diet/Create.cshtml", viewModel);
        }

        try
        {
            var dietPlan = viewModel.DietPlan;
            dietPlan.trainerId = trainer.trainerId;
            dietPlan.dateCreated = DateTime.Now;

            context.dietPlans.Add(dietPlan);
            await context.SaveChangesAsync();

            var client = await context.clients.FindAsync(dietPlan.clientId);
            if (client != null)
            {
                context.notifications.Add(new Notification
                {
                    userId = client.userId,
                    message = $"New diet plan assigned: {dietPlan.name}",
                    date = DateTime.Now,
                    isRead = false
                });
                await context.SaveChangesAsync();
            }

            TempData["Success"] = $"Diet plan '{dietPlan.name}' created successfully.";
            return RedirectToAction("routines", "Trainer");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to create plan: {ex.InnerException?.Message ?? ex.Message}";
            viewModel.AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList();
            return View("~/Views/Trainer/Diet/Create.cshtml", viewModel);
        }
    }

    
    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> log(int? mealId, int? planId)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var log = new MealLog
        {
            clientId = client.clientId,
            mealId = mealId,
            dietPlanId = planId,
            date = DateTime.Now
        };

        if (mealId.HasValue)
        {
            var meal = await context.meals.FindAsync(mealId.Value);
            if (meal != null) log.caloriesConsumed = meal.calories;
        }

        ViewBag.SelectedMealName = mealId.HasValue
            ? (await context.meals.FindAsync(mealId.Value))?.name
            : null;

        return View("~/Views/Client/Diet/Log.cshtml", log);
    }

    
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> log(MealLog log)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        if (!ModelState.IsValid)
        {
            if (log.mealId.HasValue)
                ViewBag.SelectedMealName = (await context.meals.FindAsync(log.mealId.Value))?.name;
            return View("~/Views/Client/Diet/Log.cshtml", log);
        }

        try
        {
            log.clientId = client.clientId;
            log.date = DateTime.Now;

            context.mealLogs.Add(log);
            await context.SaveChangesAsync();

            if (client.trainerId.HasValue)
            {
                var trainer = await context.trainers.FindAsync(client.trainerId);
                if (trainer != null)
                {
                    var mealName = log.mealId.HasValue
                        ? (await context.meals.FindAsync(log.mealId.Value))?.name ?? "meal"
                        : "meal";
                    context.notifications.Add(new Notification
                    {
                        userId = trainer.userId,
                        message = $"{client.name} logged '{mealName}' — {log.caloriesConsumed} cal.",
                        date = DateTime.Now,
                        isRead = false
                    });
                    await context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"Meal logged: {log.caloriesConsumed} cal.";
            return RedirectToAction("index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
            if (log.mealId.HasValue)
                ViewBag.SelectedMealName = (await context.meals.FindAsync(log.mealId.Value))?.name;
            return View("~/Views/Client/Diet/Log.cshtml", log);
        }
    }
}