using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels.Shared; 
using HealthTrack.ViewModels.Client; 
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class AppointmentController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public AppointmentController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    // GET: /Appointment/Index
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var viewModel = new AppointmentIndexViewModel
        {
            IsTrainer = User.IsInRole("Trainer")
        };

        if (viewModel.IsTrainer)
        {
            var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
            if (trainer == null) return NotFound();

            viewModel.Appointments = context.appointments
                .Include(a => a.client)
                .Where(a => a.trainerId == trainer.trainerId)
                .ToList();
        }
        else
        {
            var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
            if (client == null) return NotFound();

            viewModel.Appointments = context.appointments
                .Include(a => a.trainer)
                .Where(a => a.clientId == client.clientId)
                .ToList();
        }

        // Calculate counts for the dashboard cards
        viewModel.TotalCount = viewModel.Appointments.Count;
        viewModel.PendingCount = viewModel.Appointments.Count(a => a.status == "Pending");
        viewModel.ConfirmedCount = viewModel.Appointments.Count(a => a.status == "Confirmed");
        viewModel.CancelledCount = viewModel.Appointments.Count(a => a.status == "Cancelled");

        // Point to the new folder structure
        return View("~/Views/Client/Appointment/Index.cshtml", viewModel);
    }

    // GET: /Appointment/Book
    [HttpGet]
    [Authorize(Roles = "Client")]
    public IActionResult book()
    {
        var viewModel = new BookAppointmentViewModel
        {
            AvailableTrainers = context.trainers.ToList()
        };
        
        return View("~/Views/Client/Appointment/Book.cshtml", viewModel);
    }

    // POST: /Appointment/Book
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> book(BookAppointmentViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.AvailableTrainers = context.trainers.ToList();
            return View("~/Views/Client/Appointment/Book.cshtml", viewModel);
        }

        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        
        if (client == null) return NotFound();

        // Map ViewModel to Entity
        var appointment = viewModel.Appointment;
        appointment.clientId = client.clientId;
        appointment.status = "Pending";

        context.appointments.Add(appointment);
        await context.SaveChangesAsync();

        // Notify Trainer
        var trainer = await context.trainers.FindAsync(appointment.trainerId);
        if (trainer != null)
        {
            context.notifications.Add(new Notification
            {
                userId = trainer.userId,
                message = $"New appointment request from {client.name} for {appointment.dateTime:MMM d, yyyy h:mm tt}.",
                date = DateTime.Now,
                isRead = false
            });
            await context.SaveChangesAsync();
        }

        return RedirectToAction("index");
    }

    // POST: /Appointment/UpdateStatus
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> updateStatus(int id, string status)
    {
        var appointment = await context.appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.status = status;
            await context.SaveChangesAsync();
            
            var client = await context.clients.FindAsync(appointment.clientId);
            if (client != null)
            {
                context.notifications.Add(new Notification
                {
                    userId = client.userId,
                    message = $"Your appointment on {appointment.dateTime:MMM d} has been {status}.",
                    date = DateTime.Now,
                    isRead = false
                });
                await context.SaveChangesAsync();
            }
        }
        return RedirectToAction("index");
    }
}