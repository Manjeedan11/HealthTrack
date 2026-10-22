using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels.Trainer;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class WorkoutController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public WorkoutController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    // GET: /Workout/Index
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        System.Collections.Generic.List<WorkoutPlan> plans;

        if (User.IsInRole("Trainer"))
        {
            var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
            if (trainer == null) return NotFound();

            plans = context.workoutPlans
                .Include(p => p.exercises)
                .Where(p => p.trainerId == trainer.trainerId)
                .ToList();
        }
        else
        {
            var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
            if (client == null) return NotFound();

            plans = context.workoutPlans
                .Include(p => p.exercises)
                .Where(p => p.clientId == client.clientId)
                .ToList();
        }

        return View("~/Views/Client/Workout/Index.cshtml", plans);
    }

    // GET: /Workout/Create  — Trainer only
    [HttpGet]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> create()
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var viewModel = new CreateWorkoutPlanViewModel
        {
            AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList()
        };

        return View("~/Views/Trainer/Workout/Create.cshtml", viewModel);
    }

    // POST: /Workout/Create
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> create(CreateWorkoutPlanViewModel viewModel)
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        if (!ModelState.IsValid)
        {
            viewModel.AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList();
            return View("~/Views/Trainer/Workout/Create.cshtml", viewModel);
        }

        try
        {
            var plan = viewModel.WorkoutPlan;
            plan.trainerId = trainer.trainerId;
            plan.dateCreated = DateTime.Now;

            context.workoutPlans.Add(plan);
            await context.SaveChangesAsync();

            // Notify assigned client
            var client = await context.clients.FindAsync(plan.clientId);
            if (client != null)
            {
                context.notifications.Add(new Notification
                {
                    userId = client.userId,
                    message = $"New workout plan assigned: {plan.name}",
                    date = DateTime.Now,
                    isRead = false
                });
                await context.SaveChangesAsync();
            }

            TempData["Success"] = $"Workout plan '{plan.name}' created successfully.";
            return RedirectToAction("index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to create plan: {ex.InnerException?.Message ?? ex.Message}";
            viewModel.AvailableClients = context.clients
                .Where(c => c.trainerId == trainer.trainerId)
                .ToList();
            return View("~/Views/Trainer/Workout/Create.cshtml", viewModel);
        }
    }
}