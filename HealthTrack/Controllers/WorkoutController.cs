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
public class WorkoutController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;
    private readonly IHealthMetricSyncService healthMetricSync;

    public WorkoutController(
        HealthTrackDbContext context,
        UserManager<User> userManager,
        IHealthMetricSyncService healthMetricSync)
    {
        this.context = context;
        this.userManager = userManager;
        this.healthMetricSync = healthMetricSync;
    }

    // GET: /Workout/Index — shared view for Client + Trainer
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        List<WorkoutPlan> plans;

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

            // Recent workout logs for the client
            ViewBag.RecentLogs = context.workoutLogs
                .Include(w => w.exercise)
                .Where(w => w.clientId == client.clientId)
                .OrderByDescending(w => w.date)
                .Take(10)
                .ToList();
        }

        return View("~/Views/Client/Workout/Index.cshtml", plans);
    }

    // GET: /Workout/Create — Trainer only
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
            return RedirectToAction("routines", "Trainer");
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

    // GET: /Workout/Log?exerciseId=X&planId=Y — Client only
    [HttpGet]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> log(int? exerciseId, int? planId)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var log = new WorkoutLog
        {
            clientId = client.clientId,
            exerciseId = exerciseId,
            workoutPlanId = planId,
            date = DateTime.Now
        };

        if (exerciseId.HasValue)
        {
            var ex = await context.exercises.FindAsync(exerciseId.Value);
            if (ex != null) log.durationMinutes = ex.duration;
        }

        ViewBag.SelectedExerciseName = exerciseId.HasValue
            ? (await context.exercises.FindAsync(exerciseId.Value))?.name
            : null;

        return View("~/Views/Client/Workout/Log.cshtml", log);
    }

    // POST: /Workout/Log — Client only
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> log(WorkoutLog log)
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        if (!ModelState.IsValid)
        {
            if (log.exerciseId.HasValue)
                ViewBag.SelectedExerciseName = (await context.exercises.FindAsync(log.exerciseId.Value))?.name;
            return View("~/Views/Client/Workout/Log.cshtml", log);
        }

        try
        {
            log.clientId = client.clientId;
            log.date = DateTime.Now;

            context.workoutLogs.Add(log);
            await context.SaveChangesAsync();

            
            await healthMetricSync.syncTodaysCalories(client.clientId);

           
            if (client.trainerId.HasValue)
            {
                var trainer = await context.trainers.FindAsync(client.trainerId);
                if (trainer != null)
                {
                    var exerciseName = log.exerciseId.HasValue
                        ? (await context.exercises.FindAsync(log.exerciseId.Value))?.name ?? "workout"
                        : "workout";
                    context.notifications.Add(new Notification
                    {
                        userId = trainer.userId,
                        message = $"{client.name} completed '{exerciseName}' — {log.durationMinutes} min, {log.caloriesBurned} cal.",
                        date = DateTime.Now,
                        isRead = false
                    });
                    await context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"Workout logged: {log.caloriesBurned} cal. Health metric updated.";
            return RedirectToAction("index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
            if (log.exerciseId.HasValue)
                ViewBag.SelectedExerciseName = (await context.exercises.FindAsync(log.exerciseId.Value))?.name;
            return View("~/Views/Client/Workout/Log.cshtml", log);
        }
    }
}