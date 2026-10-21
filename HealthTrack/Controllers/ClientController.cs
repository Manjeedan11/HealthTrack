using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels;
using System.Linq;

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

    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var latestMetric = context.healthMetrics
            .Where(h => h.clientId == client.clientId)
            .OrderByDescending(h => h.date)
            .FirstOrDefault();

        var viewModel = new ClientDashboardViewModel
        {
            ClientName = client.name,
            FirstName = client.name.Split(' ')[0],
            Initials = string.Join("", client.name.Split(' ').Select(n => n[0])).ToUpper(),
            
            // KPIs
            BodyWeight = latestMetric?.weight ?? 0,
            WeightUnit = "kg",
            Bmi = latestMetric?.bmi ?? 0,
            BmiCategory = "Normal", 
            CaloriesBurned = latestMetric?.caloriesBurned ?? 0,
            
            // Workout
            HasWorkout = context.workoutPlans.Any(w => w.clientId == client.clientId),
            WorkoutTitle = "Full Body Strength",
            WorkoutSchedule = "Today at 5:00 PM",
            
            // Empty states for the rest
            HasAppointment = false,
            HasDietPlan = false,
            HasProgress = false,
            HasFeedback = false
        };

        return View(viewModel);
    }
}