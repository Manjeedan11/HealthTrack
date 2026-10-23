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

    
    [HttpGet]
    public async Task<IActionResult> clientDetails(int id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var client = context.clients.FirstOrDefault(c => c.clientId == id);
        if (client == null) return NotFound();

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

    
    [HttpGet]
    public async Task<IActionResult> reports(DateTime? dateFrom, DateTime? dateTo, string? reportType)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        ViewData["TrainerName"] = trainer.name;
        ViewData["TrainerInitials"] = string.Join("",
            trainer.name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => n[0])).ToUpper();

        var from = dateFrom ?? DateTime.Now.AddDays(-30);
        var to = dateTo ?? DateTime.Now;
        var type = string.IsNullOrEmpty(reportType) ? "all" : reportType.ToLowerInvariant();

        var assignedClientIds = context.clients
            .Where(c => c.trainerId == trainer.trainerId)
            .Select(c => c.clientId)
            .ToList();

        var workoutLogs = context.workoutLogs
            .Where(w => assignedClientIds.Contains(w.clientId) && w.date >= from && w.date <= to)
            .ToList();

        var mealLogs = context.mealLogs
            .Where(m => assignedClientIds.Contains(m.clientId) && m.date >= from && m.date <= to)
            .ToList();

        var metrics = context.healthMetrics
            .Where(h => assignedClientIds.Contains(h.clientId) && h.date >= from && h.date <= to)
            .ToList();

        
        var clients = context.clients.Where(c => c.trainerId == trainer.trainerId).ToList();
        var clientPerf = clients.Select(c =>
        {
            var wc = workoutLogs.Count(w => w.clientId == c.clientId);
            var mc = mealLogs.Count(m => m.clientId == c.clientId);
            var hm = metrics.Count(h => h.clientId == c.clientId);
            return new ClientPerformanceRow
            {
                ClientId = c.clientId,
                ClientName = c.name,
                Initials = string.Join("", c.name.Split(' ').Select(n => n[0])).ToUpper(),
                WorkoutLogs = wc,
                MealLogs = mc,
                HealthMetrics = hm,
                TotalActivity = wc + mc + hm
            };
        }).OrderByDescending(c => c.TotalActivity).ToList();

        
        var colors = new[] { "#0d6e5a", "#0284c7", "#f59e0b", "#8b5cf6", "#ef4444", "#10b981" };

        var popularWorkouts = context.workoutLogs
            .Where(w => assignedClientIds.Contains(w.clientId) && w.date >= from && w.date <= to)
            .Include(w => w.workoutPlan)
            .ToList()
            .Where(w => w.workoutPlan != null)
            .GroupBy(w => w.workoutPlan!.name)
            .Select((g, i) => new RoutineUsageItem
            {
                Name = g.Key,
                UsageCount = g.Count(),
                Color = colors[i % colors.Length]
            })
            .OrderByDescending(x => x.UsageCount)
            .Take(6)
            .ToList();

        var popularDiets = context.mealLogs
            .Where(m => assignedClientIds.Contains(m.clientId) && m.date >= from && m.date <= to)
            .Include(m => m.dietPlan)
            .ToList()
            .Where(m => m.dietPlan != null)
            .GroupBy(m => m.dietPlan!.name)
            .Select((g, i) => new RoutineUsageItem
            {
                Name = g.Key,
                UsageCount = g.Count(),
                Color = colors[i % colors.Length]
            })
            .OrderByDescending(x => x.UsageCount)
            .Take(6)
            .ToList();

        
        var days = new System.Collections.Generic.List<DateTime>();
        var cur = from.Date;
        while (cur <= to.Date && days.Count < 30)
        {
            days.Add(cur);
            cur = cur.AddDays(1);
        }

        var engagementByDay = days.Select(d => new EngagementDayItem
        {
            Date = d,
            WorkoutLogs = workoutLogs.Count(w => w.date.Date == d),
            MealLogs = mealLogs.Count(m => m.date.Date == d),
            Metrics = metrics.Count(h => h.date.Date == d),
            Total = workoutLogs.Count(w => w.date.Date == d)
                  + mealLogs.Count(m => m.date.Date == d)
                  + metrics.Count(h => h.date.Date == d)
        }).ToList();

        var viewModel = new TrainerReportsViewModel
        {
            DateFrom = from,
            DateTo = to,
            ReportType = type,
            TotalWorkoutLogs = workoutLogs.Count,
            TotalMealLogs = mealLogs.Count,
            TotalMetrics = metrics.Count,
            TotalEngagement = workoutLogs.Count + mealLogs.Count + metrics.Count,
            ClientPerformance = clientPerf,
            PopularWorkoutPlans = popularWorkouts,
            PopularDietPlans = popularDiets,
            EngagementByDay = engagementByDay
        };

        return View("~/Views/Trainer/Reports/Reports.cshtml", viewModel);
    }

    
    [HttpGet]
    public async Task<IActionResult> exportCsv(DateTime? dateFrom, DateTime? dateTo)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var from = dateFrom ?? DateTime.Now.AddDays(-30);
        var to = dateTo ?? DateTime.Now;

        var assignedClientIds = context.clients
            .Where(c => c.trainerId == trainer.trainerId)
            .Select(c => c.clientId)
            .ToList();

        var clients = context.clients.Where(c => c.trainerId == trainer.trainerId).ToList();
        var workoutLogs = context.workoutLogs
            .Where(w => assignedClientIds.Contains(w.clientId) && w.date >= from && w.date <= to)
            .ToList();
        var mealLogs = context.mealLogs
            .Where(m => assignedClientIds.Contains(m.clientId) && m.date >= from && m.date <= to)
            .ToList();
        var metrics = context.healthMetrics
            .Where(h => assignedClientIds.Contains(h.clientId) && h.date >= from && h.date <= to)
            .ToList();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("HealthTrack Performance Report");
        csv.AppendLine($"Trainer,{trainer.name}");
        csv.AppendLine($"Date Range,{from:yyyy-MM-dd},{to:yyyy-MM-dd}");
        csv.AppendLine();
        csv.AppendLine("CLIENT PERFORMANCE");
        csv.AppendLine("Client,Workout Logs,Meal Logs,Health Metrics,Total Activity");
        foreach (var c in clients)
        {
            var wc = workoutLogs.Count(w => w.clientId == c.clientId);
            var mc = mealLogs.Count(m => m.clientId == c.clientId);
            var hm = metrics.Count(h => h.clientId == c.clientId);
            csv.AppendLine($"{c.name},{wc},{mc},{hm},{wc + mc + hm}");
        }
        csv.AppendLine();
        csv.AppendLine("POPULAR WORKOUT PLANS");
        csv.AppendLine("Plan Name,Usage Count");
        var wGroups = workoutLogs.Where(w => w.workoutPlan != null)
            .GroupBy(w => w.workoutPlan!.name)
            .OrderByDescending(g => g.Count());
        foreach (var g in wGroups)
            csv.AppendLine($"{g.Key},{g.Count()}");
        csv.AppendLine();
        csv.AppendLine("POPULAR DIET PLANS");
        csv.AppendLine("Plan Name,Usage Count");
        var dGroups = mealLogs.Where(m => m.dietPlan != null)
            .GroupBy(m => m.dietPlan!.name)
            .OrderByDescending(g => g.Count());
        foreach (var g in dGroups)
            csv.AppendLine($"{g.Key},{g.Count()}");

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"HealthTrack-Report-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }
}