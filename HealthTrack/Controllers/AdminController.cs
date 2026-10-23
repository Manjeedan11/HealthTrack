using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthTrack.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public AdminController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    private async Task SetAdminViewData()
    {
        var user = await userManager.GetUserAsync(User);
        if (user != null)
        {
            ViewData["AdminName"] = user.Email ?? "Administrator";
            ViewData["AdminInitials"] = "AD";
        }
    }

    
    public async Task<IActionResult> index()
    {
        await SetAdminViewData();

        var users = context.Users.ToList();
        var clients = context.clients.ToList();
        var trainers = context.trainers.ToList();
        var appointments = context.appointments.OrderByDescending(a => a.dateTime).ToList();

        var recentUsers = users
            .OrderByDescending(u => u.Id)
            .Take(6)
            .Select(u =>
            {
                var client = clients.FirstOrDefault(c => c.userId == u.Id);
                var trainer = trainers.FirstOrDefault(t => t.userId == u.Id);
                var name = client?.name ?? trainer?.name ?? u.Email ?? "Unknown";
                var initials = string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(n => n[0])).ToUpper();
                if (string.IsNullOrEmpty(initials)) initials = "??";
                return new AdminRecentUser
                {
                    Name = name,
                    Email = u.Email ?? "",
                    Role = u.role,
                    Initials = initials.Length > 2 ? initials.Substring(0, 2) : initials,
                    DisplayId = $"U-{u.Id.Substring(0, Math.Min(6, u.Id.Length))}"
                };
            })
            .ToList();

        var recentAppointments = appointments
            .Take(5)
            .Select(a =>
            {
                var client = clients.FirstOrDefault(c => c.clientId == a.clientId);
                var trainer = trainers.FirstOrDefault(t => t.trainerId == a.trainerId);
                return new AdminRecentAppointment
                {
                    ClientName = client?.name ?? "Unknown Client",
                    TrainerName = trainer?.name ?? "Unknown Trainer",
                    TimeFormatted = a.dateTime.ToString("MMM dd, h:mm tt"),
                    Status = a.status,
                    ServiceType = "Consultation"
                };
            })
            .ToList();

        var adminUser = await userManager.GetUserAsync(User);
        var adminName = adminUser?.Email ?? "Administrator";

        var viewModel = new AdminDashboardViewModel
        {
            AdminName = adminName,
            FirstName = adminName.Split('@')[0],
            Initials = "AD",
            IsDemonstrationData = false,
            TotalUsers = users.Count,
            TotalClients = clients.Count,
            TotalTrainers = trainers.Count,
            TotalAppointments = appointments.Count,
            TotalWorkoutPlans = context.workoutPlans.Count(),
            TotalDietPlans = context.dietPlans.Count(),
            RecentUsers = recentUsers,
            RecentAppointments = recentAppointments
        };

        ViewData["AdminName"] = adminName;
        ViewData["AdminInitials"] = "AD";

        return View("~/Views/Admin/Index.cshtml", viewModel);
    }

    
    public async Task<IActionResult> users()
    {
        await SetAdminViewData();

        var users = context.Users.ToList();
        var list = new List<UserListViewModel>();

        foreach (var u in users)
        {
            string displayName = u.Email ?? "Unknown";
            var client = context.clients.FirstOrDefault(c => c.userId == u.Id);
            var trainer = context.trainers.FirstOrDefault(t => t.userId == u.Id);
            if (client != null) displayName = client.name;
            else if (trainer != null) displayName = trainer.name;

            list.Add(new UserListViewModel
            {
                userId = u.Id,
                email = u.Email ?? "",
                role = u.role,
                displayName = displayName
            });
        }

        return View("~/Views/Admin/Users.cshtml", list);
    }

    
    [HttpGet]
    public async Task<IActionResult> createTrainer()
    {
        await SetAdminViewData();
        return View("~/Views/Admin/CreateTrainer.cshtml");
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> createTrainer(CreateTrainerViewModel model)
    {
        await SetAdminViewData();

        if (!ModelState.IsValid)
            return View("~/Views/Admin/CreateTrainer.cshtml", model);

        var user = new User
        {
            UserName = model.email,
            Email = model.email,
            role = "Trainer"
        };

        var result = await userManager.CreateAsync(user, model.password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Trainer");
            context.trainers.Add(new Trainer
            {
                userId = user.Id,
                name = model.name,
                specialization = model.specialization,
                bio = model.bio
            });
            await context.SaveChangesAsync();

            TempData["Success"] = $"Trainer account '{model.name}' created successfully.";
            return RedirectToAction("users");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View("~/Views/Admin/CreateTrainer.cshtml", model);
    }

    
    [HttpGet]
    public async Task<IActionResult> assignTrainers()
    {
        await SetAdminViewData();

        var viewModel = new AssignTrainerViewModel
        {
            Clients = context.clients.ToList(),
            Trainers = context.trainers.ToList()
        };
        return View("~/Views/Admin/AssignTrainers.cshtml", viewModel);
    }

    // POST: /Admin/assignTrainers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> assignTrainers(int clientId, int? trainerId)
    {
        await SetAdminViewData();

        var client = context.clients.FirstOrDefault(c => c.clientId == clientId);
        if (client == null) return NotFound();

        client.trainerId = trainerId;
        context.SaveChanges();

        var trainerName = trainerId.HasValue
            ? context.trainers.FirstOrDefault(t => t.trainerId == trainerId)?.name ?? "Unknown"
            : "Unassigned";

        TempData["Success"] = $"Client '{client.name}' assigned to '{trainerName}'.";
        return RedirectToAction("assignTrainers");
    }

    
    [HttpGet]
    public async Task<IActionResult> reports(DateTime? dateFrom, DateTime? dateTo, string? reportType)
    {
        await SetAdminViewData();

        var from = dateFrom ?? DateTime.Now.AddDays(-30);
        var to = dateTo ?? DateTime.Now;
        var type = string.IsNullOrEmpty(reportType) ? "all" : reportType.ToLowerInvariant();

        var users = context.Users.ToList();
        var clients = context.clients.ToList();
        var trainers = context.trainers.ToList();

        var appointments = context.appointments
            .Where(a => a.dateTime >= from && a.dateTime <= to)
            .ToList();

        var workoutLogs = context.workoutLogs
            .Where(w => w.date >= from && w.date <= to)
            .ToList();

        var mealLogs = context.mealLogs
            .Where(m => m.date >= from && m.date <= to)
            .ToList();

        var metrics = context.healthMetrics
            .Where(h => h.date >= from && h.date <= to)
            .ToList();

        var totalAdmins = users.Count(u => u.role == "Admin");

        
        var roleBreakdown = new List<BreakdownItem>
        {
            new() { Label = "Clients",  Count = clients.Count,  Color = "#0d6e5a" },
            new() { Label = "Trainers", Count = trainers.Count, Color = "#0284c7" },
            new() { Label = "Admins",   Count = totalAdmins,    Color = "#f59e0b" }
        };

        var appointmentStatus = new List<BreakdownItem>
        {
            new() { Label = "Completed", Count = appointments.Count(a => a.status == "Completed"), Color = "#0d6e5a" },
            new() { Label = "Confirmed", Count = appointments.Count(a => a.status == "Confirmed"), Color = "#0284c7" },
            new() { Label = "Pending",   Count = appointments.Count(a => a.status == "Pending"),   Color = "#f59e0b" },
            new() { Label = "Cancelled", Count = appointments.Count(a => a.status == "Cancelled"), Color = "#ef4444" }
        };

        
        var days = new List<DateTime>();
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

        var workoutPlanNames = context.workoutPlans.ToDictionary(w => w.planId, w => w.name);
        var popularWorkoutItems = workoutLogs
            .Where(w => w.workoutPlanId.HasValue)
            .GroupBy(w => w.workoutPlanId!.Value)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .Select((x, i) => new RoutineUsageItem
            {
                Name = workoutPlanNames.TryGetValue(x.PlanId, out var name) ? name : "Unknown",
                UsageCount = x.Count,
                Color = colors[i % colors.Length]
            })
            .ToList();

        var dietPlanNames = context.dietPlans.ToDictionary(d => d.dietId, d => d.name);
        var popularDietItems = mealLogs
            .Where(m => m.dietPlanId.HasValue)
            .GroupBy(m => m.dietPlanId!.Value)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(6)
            .Select((x, i) => new RoutineUsageItem
            {
                Name = dietPlanNames.TryGetValue(x.PlanId, out var name) ? name : "Unknown",
                UsageCount = x.Count,
                Color = colors[i % colors.Length]
            })
            .ToList();

        var viewModel = new AdminReportsViewModel
        {
            DateFrom = from,
            DateTo = to,
            ReportType = type,
            TotalUsers = users.Count,
            TotalClients = clients.Count,
            TotalTrainers = trainers.Count,
            TotalAdmins = totalAdmins,
            TotalAppointments = appointments.Count,
            TotalWorkoutLogs = workoutLogs.Count,
            TotalMealLogs = mealLogs.Count,
            TotalHealthMetrics = metrics.Count,
            TotalEngagement = workoutLogs.Count + mealLogs.Count + metrics.Count,
            TotalWorkoutPlans = context.workoutPlans.Count(),
            TotalDietPlans = context.dietPlans.Count(),
            RoleBreakdown = roleBreakdown,
            AppointmentStatus = appointmentStatus,
            EngagementByDay = engagementByDay,
            ClientPerformance = clientPerf,
            PopularWorkoutPlans = popularWorkoutItems,
            PopularDietPlans = popularDietItems
        };

        return View("~/Views/Admin/Reports.cshtml", viewModel);
    }

    
    [HttpGet]
    public async Task<IActionResult> exportCsv(DateTime? dateFrom, DateTime? dateTo)
    {
        await SetAdminViewData();

        var from = dateFrom ?? DateTime.Now.AddDays(-30);
        var to = dateTo ?? DateTime.Now;

        var users = context.Users.ToList();
        var clients = context.clients.ToList();
        var trainers = context.trainers.ToList();
        var appointments = context.appointments.Where(a => a.dateTime >= from && a.dateTime <= to).ToList();
        var workoutLogs = context.workoutLogs.Where(w => w.date >= from && w.date <= to).ToList();
        var mealLogs = context.mealLogs.Where(m => m.date >= from && m.date <= to).ToList();
        var metrics = context.healthMetrics.Where(h => h.date >= from && h.date <= to).ToList();

        var csv = new StringBuilder();
        csv.AppendLine("HealthTrack Admin Report");
        csv.AppendLine($"Date Range,{from:yyyy-MM-dd},{to:yyyy-MM-dd}");
        csv.AppendLine();

        csv.AppendLine("SYSTEM USAGE");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Users,{users.Count}");
        csv.AppendLine($"Clients,{clients.Count}");
        csv.AppendLine($"Trainers,{trainers.Count}");
        csv.AppendLine($"Admins,{users.Count(u => u.role == "Admin")}");
        csv.AppendLine($"Appointments,{appointments.Count}");
        csv.AppendLine($"Workout Plans,{context.workoutPlans.Count()}");
        csv.AppendLine($"Diet Plans,{context.dietPlans.Count()}");
        csv.AppendLine();

        csv.AppendLine("APPOINTMENT STATUS");
        csv.AppendLine("Status,Count");
        csv.AppendLine($"Completed,{appointments.Count(a => a.status == "Completed")}");
        csv.AppendLine($"Confirmed,{appointments.Count(a => a.status == "Confirmed")}");
        csv.AppendLine($"Pending,{appointments.Count(a => a.status == "Pending")}");
        csv.AppendLine($"Cancelled,{appointments.Count(a => a.status == "Cancelled")}");
        csv.AppendLine();

        csv.AppendLine("ENGAGEMENT SUMMARY");
        csv.AppendLine("Type,Count");
        csv.AppendLine($"Workout Logs,{workoutLogs.Count}");
        csv.AppendLine($"Meal Logs,{mealLogs.Count}");
        csv.AppendLine($"Health Metrics,{metrics.Count}");
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
        var wGroups = workoutLogs.Where(w => w.workoutPlanId.HasValue)
            .GroupBy(w => w.workoutPlanId!.Value)
            .OrderByDescending(g => g.Count());
        var workoutNames = context.workoutPlans.ToDictionary(w => w.planId, w => w.name);
        foreach (var g in wGroups)
            csv.AppendLine($"{workoutNames.GetValueOrDefault(g.Key, "Unknown")},{g.Count()}");
        csv.AppendLine();

        csv.AppendLine("POPULAR DIET PLANS");
        csv.AppendLine("Plan Name,Usage Count");
        var dGroups = mealLogs.Where(m => m.dietPlanId.HasValue)
            .GroupBy(m => m.dietPlanId!.Value)
            .OrderByDescending(g => g.Count());
        var dietNames = context.dietPlans.ToDictionary(d => d.dietId, d => d.name);
        foreach (var g in dGroups)
            csv.AppendLine($"{dietNames.GetValueOrDefault(g.Key, "Unknown")},{g.Count()}");

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"HealthTrack-Admin-Report-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }
}