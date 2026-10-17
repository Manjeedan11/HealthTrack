using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class FeedbackController : Controller
{
    private readonly IFeedbackService service;
    private readonly UserManager<User> userManager;
    private readonly HealthTrackDbContext context;

    public FeedbackController(IFeedbackService service,
                              UserManager<User> userManager,
                              HealthTrackDbContext context)
    {
        this.service = service;
        this.userManager = userManager;
        this.context = context;
    }

    [Authorize(Roles = "Client")]
    public async Task<IActionResult> myFeedback()
    {
        var user = await userManager.GetUserAsync(User);
        var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
        if (client == null) return NotFound();

        var feedbacks = await service.getClientFeedback(client.clientId);
        return View(feedbacks);
    }

    [Authorize(Roles = "Trainer")]
    public IActionResult give(int clientId) => View(new Feedback { clientId = clientId });

    [HttpPost]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> give(Feedback feedback)
    {
        if (!ModelState.IsValid) return View(feedback);

        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        feedback.trainerId = trainer.trainerId;
        await service.giveFeedback(feedback);
        return RedirectToAction("index", "Trainer");
    }
}
