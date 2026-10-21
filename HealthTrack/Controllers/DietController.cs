using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthTrack.ViewModels.Trainer;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
public class DietController : Controller
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public DietController(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    
    public async Task<IActionResult> index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        List<DietPlan> plans;

        if (User.IsInRole("Trainer"))
        {
            var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
            if (trainer == null) return NotFound();

            
            plans = context.dietPlans
                .Include(p => p.meals)
                .Where(p => p.trainerId == trainer.trainerId)
                .ToList();
        }
        else
        {
            var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
            if (client == null) return NotFound();

            
            plans = context.dietPlans
                .Include(p => p.meals)
                .Where(p => p.clientId == client.clientId)
                .ToList();
        }

       
        return View("~/Views/Client/Diet/Index.cshtml", plans);
    }

 
   
    [Authorize(Roles = "Trainer")]
    public IActionResult create()
    {
        var viewModel = new CreateDietPlanViewModel
        {
            AvailableClients = context.clients.ToList() 
        };
        
        return View("~/Views/Client/Diet/Create.cshtml", viewModel);
    }

    
    [HttpPost]
    [Authorize(Roles = "Trainer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> create(CreateDietPlanViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
          
            viewModel.AvailableClients = context.clients.ToList(); 
            return View("~/Views/Client/Diet/Create.cshtml", viewModel);
        }

        var user = await userManager.GetUserAsync(User);
        var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
        if (trainer == null) return NotFound();

        var dietPlan = viewModel.DietPlan;
        dietPlan.trainerId = trainer.trainerId;
        dietPlan.dateCreated = DateTime.Now;

        context.dietPlans.Add(dietPlan);
        await context.SaveChangesAsync();

        // Notify client
        var client = await context.clients.FindAsync(dietPlan.clientId);
        if (client != null)
        {
            context.notifications.Add(new Notification
            {
                userId = client.userId,
                message = $"New diet plan assigned: {dietPlan.name}",
                date = DateTime.Now,
                isRead = false
            });
            await context.SaveChangesAsync();
        }

        return RedirectToAction("index");
    }
}