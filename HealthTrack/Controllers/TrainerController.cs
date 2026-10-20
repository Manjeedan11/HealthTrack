using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels;
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
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var clients = context.clients.ToList();
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

            // Map Clients (calculating initials, ID, and finding their latest weight)
            AssignedClients = clients.Select(c => {
                var latestMetric = context.healthMetrics
                    .Where(h => h.clientId == c.clientId)
                    .OrderByDescending(h => h.date)
                    .FirstOrDefault();
                    
                return new AssignedClientItem
                {
                    ClientId = c.clientId,
                    Name = c.name,
                    Initials = string.Join("", c.name.Split(' ').Select(n => n[0])).ToUpper(),
                    DisplayId = $"C-{c.clientId:D3}", // e.g., C-001
                    LatestWeight = latestMetric?.weight,
                    WeightUnit = "kg"
                };
            }).ToList(),

            // Map Appointments
            UpcomingAppointments = appointments.Select(a => new UpcomingAppointmentItem
            {
                ClientName = clients.FirstOrDefault(c => c.clientId == a.clientId)?.name ?? "Unknown Client",
                TimeFormatted = a.dateTime.ToString("MMM dd, h:mm tt"),
                Status = a.status,
                ServiceType = "Consultation"
            }).ToList(),

            // Map Progress Summaries (showing up to 3 recent updates)
            ProgressSummaries = clients.Take(3).Select(c => {
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

        return View(viewModel);
    }
}