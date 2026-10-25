using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;   
using HealthTrack.ViewModels.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Client")]
public class ClientController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public ClientController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    // GET: /Client/Index — Client Dashboard
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        // Latest health metric — contains auto-computed calories from sync service
        var latestMetric = context.healthMetrics
            .Where(h => h.clientId == client.clientId)
            .OrderByDescending(h => h.date)
            .FirstOrDefault();

        // Recent workout logs (last 5)
        var recentWorkouts = context.workoutLogs
            .Include(w => w.exercise)
            .Where(w => w.clientId == client.clientId)
            .OrderByDescending(w => w.date)
            .Take(5)
            .ToList();

        // Recent meal logs (last 5)
        var recentMeals = context.mealLogs
            .Include(m => m.meal)
            .Where(m => m.clientId == client.clientId)
            .OrderByDescending(m => m.date)
            .Take(5)
            .ToList();

        // Today's appointments
        var todayAppointment = context.appointments
            .Include(a => a.trainer)
            .Where(a => a.clientId == client.clientId
                     && a.dateTime >= DateTime.Today
                     && a.dateTime < DateTime.Today.AddDays(1)
                     && a.status != "Cancelled")
            .OrderBy(a => a.dateTime)
            .FirstOrDefault();

        // Today's active diet plan
        var dietPlan = context.dietPlans
            .Include(d => d.meals)
            .Where(d => d.clientId == client.clientId)
            .OrderByDescending(d => d.dateCreated)
            .FirstOrDefault();

        // Latest feedback from trainer
        var latestFeedback = context.feedbacks
            .Include(f => f.trainer)
            .Where(f => f.clientId == client.clientId)
            .OrderByDescending(f => f.date)
            .FirstOrDefault();

        // Build ViewModel
        var viewModel = new ClientDashboardViewModel
        {
            ClientName = client.name,
            FirstName = client.name.Split(' ')[0],
            Initials = string.Join("", client.name.Split(' ').Select(n => n[0])).ToUpper(),
            IsDemonstrationData = false,

            // Body composition from latest metric
            BodyWeight = latestMetric?.weight,
            WeightUnit = "kg",
            Bmi = latestMetric?.bmi,
            BmiUnit = "kg/m²",
            BmiCategory = latestMetric != null ? GetBmiCategory(latestMetric.bmi) : null,

            // Auto-calculated calories from sync service
            CaloriesBurned = latestMetric?.caloriesBurned ?? 0,
            CaloriesConsumed = latestMetric?.caloriesConsumed ?? 0,   // NET intake
            CaloriesGoal = null,

            // Appointment
            HasAppointment = todayAppointment != null,
            AppointmentTitle = todayAppointment != null
                ? $"Consultation with {todayAppointment.trainer?.name}"
                : null,
            AppointmentStatus = todayAppointment?.status,
            AppointmentProvider = todayAppointment?.trainer?.name,
            AppointmentSchedule = todayAppointment?.dateTime.ToString("MMM d · h:mm tt"),

            // Diet plan
            HasDietPlan = dietPlan != null,
            DietPlanName = dietPlan?.name,
            Meals = dietPlan?.meals?.Select(m => new MealItem
            {
                MealType = m.mealTime,
                MealName = m.name,
                IsCompleted = false,
                TimeOrStatus = $"{m.calories} kcal"
            }).ToList() ?? new System.Collections.Generic.List<MealItem>(),

            // Feedback
            HasFeedback = latestFeedback != null,
            FeedbackComment = latestFeedback?.comment,
            FeedbackMeta = latestFeedback != null
                ? $"— {latestFeedback.trainer?.name}, {latestFeedback.date:MMM d}"
                : null,

            // Reminders
            Reminders = new System.Collections.Generic.List<ReminderItem>()
        };

        return View("~/Views/Client/Index.cshtml", viewModel);
    }

    // Helper: BMI classification
    private static string GetBmiCategory(float bmi)
    {
        if (bmi <= 0) return null;
        if (bmi < 18.5f) return "Underweight";
        if (bmi < 25.0f) return "Normal";
        if (bmi < 30.0f) return "Overweight";
        return "Obese";
    }
}