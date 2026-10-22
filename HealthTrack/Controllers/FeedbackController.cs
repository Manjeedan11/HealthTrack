using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class FeedbackController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public FeedbackController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    // GET: /Feedback/MyFeedback — Client sees feedback received
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> myFeedback()
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var feedbacks = context.feedbacks
            .Include(f => f.trainer)
            .Include(f => f.appointment)
            .Where(f => f.clientId == client.clientId)
            .OrderByDescending(f => f.date)
            .ToList();

        return View("~/Views/Client/Feedback/MyFeedback.cshtml", feedbacks);
    }

    // GET: /Feedback/Give?appointmentId=X  OR  ?clientId=X
    [HttpGet]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> give(int? clientId, int? appointmentId)
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        // If appointmentId is provided, derive clientId from it
        if (appointmentId.HasValue)
        {
            var appt = await context.appointments
                .Include(a => a.client)
                .FirstOrDefaultAsync(a => a.appointmentId == appointmentId.Value);

            if (appt == null) return NotFound();

            // Only the assigned trainer can give feedback on their own appointment
            if (appt.trainerId != trainer.trainerId) return Forbid();

            clientId = appt.clientId;
        }

        if (!clientId.HasValue) return BadRequest("clientId or appointmentId required");

        var client = await context.clients.FindAsync(clientId.Value);
        if (client == null) return NotFound();

        var feedback = new Feedback
        {
            clientId = clientId.Value,
            trainerId = trainer.trainerId,
            appointmentId = appointmentId
        };

        ViewBag.ClientName = client.name;
        ViewBag.AppointmentContext = appointmentId.HasValue
            ? (await context.appointments.FindAsync(appointmentId.Value))?.dateTime.ToString("MMM d, yyyy · h:mm tt")
            : null;

        return View("~/Views/Trainer/Feedback/Give.cshtml", feedback);
    }

    // POST: /Feedback/Give
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> give(Feedback feedback)
    {
        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        // Guard: trainer must own this feedback target
        if (feedback.clientId == 0) return BadRequest("clientId required");

        // Guard: if appointmentId provided, verify it belongs to this trainer
        if (feedback.appointmentId.HasValue)
        {
            var appt = await context.appointments.FindAsync(feedback.appointmentId.Value);
            if (appt == null || appt.trainerId != trainer.trainerId) return Forbid();
        }

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(feedback.comment))
        {
            ModelState.AddModelError("comment", "Feedback text is required.");
            var client = await context.clients.FindAsync(feedback.clientId);
            ViewBag.ClientName = client?.name;
            ViewBag.AppointmentContext = feedback.appointmentId.HasValue
                ? (await context.appointments.FindAsync(feedback.appointmentId.Value))?.dateTime.ToString("MMM d, yyyy · h:mm tt")
                : null;
            return View("~/Views/Trainer/Feedback/Give.cshtml", feedback);
        }

        try
        {
            feedback.trainerId = trainer.trainerId;
            feedback.date = DateTime.Now;

            context.feedbacks.Add(feedback);
            await context.SaveChangesAsync();

            // Notify the client
            var client = await context.clients.FindAsync(feedback.clientId);
            if (client != null)
            {
                context.notifications.Add(new Notification
                {
                    userId = client.userId,
                    message = $"New feedback from {trainer.name}.",
                    date = DateTime.Now,
                    isRead = false
                });
                await context.SaveChangesAsync();
            }

            TempData["Success"] = "Feedback sent successfully.";
            return RedirectToAction("index", "Trainer");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Save failed: {ex.InnerException?.Message ?? ex.Message}");
            var client = await context.clients.FindAsync(feedback.clientId);
            ViewBag.ClientName = client?.name;
            return View("~/Views/Trainer/Feedback/Give.cshtml", feedback);
        }
    }
}