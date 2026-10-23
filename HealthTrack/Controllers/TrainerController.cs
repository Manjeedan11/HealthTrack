using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels.Trainer;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    // GET: /Trainer/Index — Trainer Dashboard
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var clients = context.clients
            .Where(c => c.trainerId == trainer.trainerId)
            .ToList();

        var appointments = context.appointments
            .Where(a => a.trainerId == trainer.trainerId && a.status != "Cancelled")
            .OrderBy(a => a.dateTime)
            .ToList();

        var viewModel = new TrainerDashboardViewModel
        {
            TrainerName = trainer.name,
            FirstName = trainer.name.Split(' ')[0],
            Initials = string.Join("", trainer.name.Split(' ').Select(n => n[0])).ToUpper(),
            IsDemonstrationData = false,
            TotalAppointments = appointments.Count,

            AssignedClients = clients.Select(c =>
            {
                var latestMetric = context.healthMetrics
                    .Where(h => h.clientId == c.clientId)
                    .OrderByDescending(h => h.date)
                    .FirstOrDefault();

                return new AssignedClientItem
                {
                    ClientId = c.clientId,
                    Name = c.name,
                    Initials = string.Join("", c.name.Split(' ').Select(n => n[0])).ToUpper(),
                    DisplayId = $"C-{c.clientId:D3}",
                    LatestWeight = latestMetric?.weight,
                    WeightUnit = "kg"
                };
            }).ToList(),

            UpcomingAppointments = appointments.Select(a => new UpcomingAppointmentItem
            {
                ClientName = clients.FirstOrDefault(c => c.clientId == a.clientId)?.name ?? "Unknown Client",
                TimeFormatted = a.dateTime.ToString("MMM dd, h:mm tt"),
                Status = a.status,
                ServiceType = "Consultation"
            }).ToList(),

            ProgressSummaries = clients.Take(3).Select(c =>
            {
                var latestMetric = context.healthMetrics
                    .Where(h => h.clientId == c.clientId)
                    .OrderByDescending(h => h.date)
                    .FirstOrDefault();

                return new ProgressSummaryItem
                {
                    ClientFirstName = c.name.Split(' ')[0],
                    Weight = latestMetric?.weight ?? 0,
                    WeightUnit = "kg",
                    StatusNote = latestMetric != null ? "Recently logged" : "No data yet"
                };
            }).ToList()
        };

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = viewModel.Initials;

        return View("~/Views/Trainer/Index.cshtml", viewModel);
    }

    // GET: /Trainer/Clients — List all assigned clients
    [HttpGet]
    public async Task<IActionResult> clients()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = string.Join("",
            trainer.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n[0])).ToUpper();

        var clients = context.clients
            .Where(c => c.trainerId == trainer.trainerId)
            .ToList();

        var clientItems = clients.Select(c =>
        {
            var latestMetric = context.healthMetrics
                .Where(h => h.clientId == c.clientId)
                .OrderByDescending(h => h.date)
                .FirstOrDefault();

            return new AssignedClientItem
            {
                ClientId = c.clientId,
                Name = c.name,
                Initials = string.Join("", c.name.Split(' ').Select(n => n[0])).ToUpper(),
                DisplayId = $"C-{c.clientId:D3}",
                LatestWeight = latestMetric?.weight,
                WeightUnit = "kg"
            };
        }).ToList();

        return View("~/Views/Trainer/Client/Clients.cshtml", clientItems);
    }

    // GET: /Trainer/clientDetails/{id} — Individual client metrics & logs
    [HttpGet]
    public async Task<IActionResult> clientDetails(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var client = context.clients.FirstOrDefault(c => c.clientId == id);
        if (client == null) return NotFound();

        // Ownership check — trainer can only view assigned clients
        if (client.trainerId != trainer.trainerId) return Forbid();

        var viewModel = new TrainerClientDetailsViewModel
        {
            ClientId = client.clientId,
            ClientName = client.name,
            Initials = string.Join("",
                client.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                           .Select(n => n[0])).ToUpper(),
            DisplayId = $"C-{client.clientId:D3}",
            Gender = client.gender,
            Height = client.height,
            DateOfBirth = client.dateOfBirth,

            RecentMetrics = context.healthMetrics
                .Where(h => h.clientId == client.clientId)
                .OrderByDescending(h => h.date)
                .Take(10)
                .ToList(),

            RecentWorkoutLogs = context.workoutLogs
                .Include(w => w.exercise)
                .Where(w => w.clientId == client.clientId)
                .OrderByDescending(w => w.date)
                .Take(10)
                .ToList(),

            RecentMealLogs = context.mealLogs
                .Include(m => m.meal)
                .Where(m => m.clientId == client.clientId)
                .OrderByDescending(m => m.date)
                .Take(10)
                .ToList(),

            TotalMetrics = context.healthMetrics.Count(h => h.clientId == client.clientId),
            TotalWorkoutLogs = context.workoutLogs.Count(w => w.clientId == client.clientId),
            TotalMealLogs = context.mealLogs.Count(m => m.clientId == client.clientId)
        };

        var latest = viewModel.RecentMetrics.FirstOrDefault();
        viewModel.LatestWeight = latest?.weight;
        viewModel.LatestBmi = latest?.bmi;
        viewModel.LatestCaloriesBurned = latest?.caloriesBurned;
        viewModel.LatestCaloriesConsumed = latest?.caloriesConsumed;
        viewModel.LastMetricDate = latest?.date;

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = string.Join("",
            trainer.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n[0])).ToUpper();

        return View("~/Views/Trainer/Client/ClientDetails.cshtml", viewModel);
    }

    // GET: /Trainer/Routines — Combined workout + diet hub
    [HttpGet]
    public async Task<IActionResult> routines()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = string.Join("",
            trainer.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n[0])).ToUpper();

        var viewModel = new ManageRoutinesViewModel
        {
            WorkoutPlans = context.workoutPlans
                .Include(w => w.exercises)
                .Where(w => w.trainerId == trainer.trainerId)
                .OrderByDescending(w => w.dateCreated)
                .ToList(),

            DietPlans = context.dietPlans
                .Include(d => d.meals)
                .Where(d => d.trainerId == trainer.trainerId)
                .OrderByDescending(d => d.dateCreated)
                .ToList()
        };

        return View("~/Views/Trainer/Routine/Routines.cshtml", viewModel);
    }

    // GET: /Trainer/Reports
    [HttpGet]
    public async Task<IActionResult> reports()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = string.Join("",
            trainer.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n[0])).ToUpper();

        var assignedClientIds = context.clients
            .Where(c => c.trainerId == trainer.trainerId)
            .Select(c => c.clientId)
            .ToList();

        ViewBag.TrainerName = trainer.name;
        ViewBag.ClientCount = assignedClientIds.Count;
        ViewBag.TotalAppointments = context.appointments.Count(a => a.trainerId == trainer.trainerId);
        ViewBag.CompletedAppointments = context.appointments.Count(a => a.trainerId == trainer.trainerId && a.status == "Completed");
        ViewBag.PendingAppointments = context.appointments.Count(a => a.trainerId == trainer.trainerId && a.status == "Pending");
        ViewBag.TotalWorkoutPlans = context.workoutPlans.Count(w => w.trainerId == trainer.trainerId);
        ViewBag.TotalDietPlans = context.dietPlans.Count(d => d.trainerId == trainer.trainerId);
        ViewBag.TotalWorkoutLogs = context.workoutLogs.Count(w => assignedClientIds.Contains(w.clientId));
        ViewBag.TotalMealLogs = context.mealLogs.Count(m => assignedClientIds.Contains(m.clientId));

        return View("~/Views/Trainer/Reports.cshtml");
    }
}