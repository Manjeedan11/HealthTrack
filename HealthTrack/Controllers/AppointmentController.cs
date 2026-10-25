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
                .Include(a => a.feedbacks).ThenInclude(f => f.trainer)
                .Where(a => a.trainerId == trainer.trainerId)
                .ToList();
        }
        else
        {
            var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
            if (client == null) return NotFound();

            viewModel.Appointments = context.appointments
                .Include(a => a.trainer)
                .Include(a => a.feedbacks).ThenInclude(f => f.trainer) 
                .Where(a => a.clientId == client.clientId)
                .ToList();
        }

       
        viewModel.TotalCount = viewModel.Appointments.Count;
        viewModel.PendingCount = viewModel.Appointments.Count(a => a.status == "Pending");
        viewModel.ConfirmedCount = viewModel.Appointments.Count(a => a.status == "Confirmed");
        viewModel.CancelledCount = viewModel.Appointments.Count(a => a.status == "Cancelled");
        viewModel.CompletedCount = viewModel.Appointments.Count(a => a.status == "Completed");

        return View("~/Views/Client/Appointment/Index.cshtml", viewModel);
    }

   
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

    [HttpPost]
    [Authorize(Roles = "Client")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> book(BookAppointmentViewModel viewModel)
   {
    Console.WriteLine("=== POST book() called ===");
    Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

    if (viewModel?.Appointment == null)
    {
        Console.WriteLine("=== viewModel.Appointment is NULL ===");
        ModelState.AddModelError("", "Form submission failed.");
        viewModel ??= new BookAppointmentViewModel();
        viewModel.AvailableTrainers = context.trainers.ToList();
        return View("~/Views/Client/Appointment/Book.cshtml", viewModel);
    }

    Console.WriteLine($"Appointment.dateTime: {viewModel.Appointment.dateTime}");
    Console.WriteLine($"Appointment.trainerId: {viewModel.Appointment.trainerId}");
    Console.WriteLine($"Appointment.notes: '{viewModel.Appointment.notes}'");

    if (viewModel.Appointment.trainerId <= 0)
    {
        Console.WriteLine("=== trainerId is 0 or negative ===");
        ModelState.AddModelError("Appointment.trainerId", "Please select a trainer.");
    }

    if (viewModel.Appointment.dateTime <= DateTime.Now)
    {
        Console.WriteLine("=== dateTime is in the past ===");
        ModelState.AddModelError("Appointment.dateTime", "Appointment must be in the future.");
    }

    if (!ModelState.IsValid)
    {
        Console.WriteLine("=== ModelState invalid, returning form ===");
        foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
            Console.WriteLine($"  Error: {err.ErrorMessage}");
        viewModel.AvailableTrainers = context.trainers.ToList();
        return View("~/Views/Client/Appointment/Book.cshtml", viewModel);
    }

    var user = await userManager.GetUserAsync(User);
    var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
    Console.WriteLine($"=== Current user ID: {user.Id}");
    Console.WriteLine($"=== Current client ID: {client?.clientId}");

    if (client == null)
    {
        Console.WriteLine("=== client not found! ===");
        return NotFound();
    }

    try
    {
        var appointment = viewModel.Appointment;
        appointment.clientId = client.clientId;
        appointment.status = "Pending";
        appointment.notes = appointment.notes ?? "";

        Console.WriteLine($"=== Saving appointment: clientId={appointment.clientId}, trainerId={appointment.trainerId}, dateTime={appointment.dateTime} ===");

        context.appointments.Add(appointment);
        await context.SaveChangesAsync();

        Console.WriteLine($"=== SAVED! appointmentId={appointment.appointmentId} ===");

        
        return RedirectToAction("index");
    }
    catch (Exception ex)
    {
        Console.WriteLine("=== EXCEPTION during save ===");
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.InnerException?.Message);
        ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
        viewModel.AvailableTrainers = context.trainers.ToList();
        return View("~/Views/Client/Appointment/Book.cshtml", viewModel);
    }
}


[HttpPost]
[Authorize(Roles = "Trainer")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> updateStatus(int id, string status)
{
    Console.WriteLine($"=== updateStatus called: id={id}, status={status} ===");

    var validStatuses = new[] { "Confirmed", "Cancelled", "Completed" };
    if (string.IsNullOrEmpty(status) || !validStatuses.Contains(status))
    {
        TempData["Error"] = "Invalid status value.";
        return RedirectToAction("index");
    }

   
    var appointment = await context.appointments.FindAsync(id);
    if (appointment == null)
    {
        TempData["Error"] = "Appointment not found.";
        return RedirectToAction("index");
    }


    var user = await userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
    if (trainer == null) return NotFound();

    if (appointment.trainerId != trainer.trainerId)
        return Forbid();

    
    if (status == "Completed" && appointment.dateTime > DateTime.Now)
    {
        TempData["Error"] = "Cannot mark a future appointment as completed.";
        return RedirectToAction("index");
    }

    if (status == "Completed" && appointment.status != "Confirmed")
    {
        TempData["Error"] = "Only confirmed appointments can be marked as completed.";
        return RedirectToAction("index");
    }

    
    try
    {
        appointment.status = status;
        await context.SaveChangesAsync();

        var client = await context.clients.FindAsync(appointment.clientId);
        if (client != null)
        {
            string verb = status == "Completed" ? "marked as completed" : status.ToLower();
            context.notifications.Add(new Notification
            {
                userId = client.userId,
                message = $"Your appointment on {appointment.dateTime:MMM d, yyyy h:mm tt} has been {verb}.",
                date = DateTime.Now,
                isRead = false
            });
            await context.SaveChangesAsync();
        }

        TempData["Success"] = $"Appointment {status.ToLower()} successfully.";
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"Update failed: {ex.InnerException?.Message ?? ex.Message}";
    }

    return RedirectToAction("index");
}

}