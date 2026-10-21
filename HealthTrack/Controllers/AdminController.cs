using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthTrack.ViewModels.Admin;
using System.Collections.Generic;
using System.Linq;
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

  
    public IActionResult index()
    {
        ViewBag.UserCount = context.Users.Count();
        ViewBag.ClientCount = context.clients.Count();
        ViewBag.TrainerCount = context.trainers.Count();
        ViewBag.AppointmentCount = context.appointments.Count();
        return View();
    }

    
    public IActionResult users()
    {
        var users = context.Users.ToList();
        var list = new List<UserListViewModel>();

        foreach (var u in users)
        {
            string displayName = u.Email;
            var client = context.clients.FirstOrDefault(c => c.userId == u.Id);
            var trainer = context.trainers.FirstOrDefault(t => t.userId == u.Id);

            if (client != null) displayName = client.name;
            else if (trainer != null) displayName = trainer.name;

            list.Add(new UserListViewModel
            {
                userId = u.Id,
                email = u.Email,
                role = u.role,
                displayName = displayName
            });
        }

        return View(list);
    }

   
    [HttpGet]
    public IActionResult createTrainer() => View();

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> createTrainer(CreateTrainerViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

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

        return View(model);
    }

   
    [HttpGet]
    public IActionResult assignTrainers()
    {
        var viewModel = new AssignTrainerViewModel
        {
            Clients = context.clients.ToList(),
            Trainers = context.trainers.ToList()
        };
        return View(viewModel);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult assignTrainers(int clientId, int? trainerId)
    {
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
}