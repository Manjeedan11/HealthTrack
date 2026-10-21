using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthTrack.ViewModels.Account;

public class AccountController : Controller
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    private readonly HealthTrackDbContext context;

    public AccountController(UserManager<User> userManager,
                             SignInManager<User> signInManager,
                             HealthTrackDbContext context)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.context = context;
    }

    [HttpGet]
    public IActionResult register() => View();

    [HttpPost]
    public async Task<IActionResult> register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        
        model.role = "Client";

        var user = new User
        {
            UserName = model.email,
            Email = model.email,
            role = "Client"
        };

        var result = await userManager.CreateAsync(user, model.password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Client");

            context.clients.Add(new Client
            {
                userId = user.Id,
                name = model.name
            });
            await context.SaveChangesAsync();

            await signInManager.SignInAsync(user, false);
            return RedirectToAction("index", "Client");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpGet]
    public IActionResult login() => View();

    [HttpPost]
    public async Task<IActionResult> login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(
            model.email, model.password, model.rememberMe, false);

        if (result.Succeeded)
        {
            var user = await userManager.FindByEmailAsync(model.email);
            if (user.role == "Admin")
                return RedirectToAction("index", "Admin");
            if (user.role == "Trainer")
                return RedirectToAction("index", "Trainer");
            return RedirectToAction("index", "Client");
        }

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}