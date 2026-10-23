using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

public class PopulateUserViewDataFilter : IAsyncActionFilter
{
    private readonly HealthTrackDbContext context;
    private readonly UserManager<User> userManager;

    public PopulateUserViewDataFilter(HealthTrackDbContext context, UserManager<User> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
    {
        var httpUser = actionContext.HttpContext.User;

        if (httpUser.Identity?.IsAuthenticated == true &&
            actionContext.Controller is Controller controller)
        {
            var user = await userManager.GetUserAsync(httpUser);

            if (user != null)
            {
                if (httpUser.IsInRole("Client"))
                {
                    var client = context.clients.FirstOrDefault(c => c.userId == user.Id);
                    if (client != null)
                        SetNames(controller, "ClientName", "ClientInitials", client.name);
                }
                else if (httpUser.IsInRole("Trainer"))
                {
                    var trainer = context.trainers.FirstOrDefault(t => t.userId == user.Id);
                    if (trainer != null)
                        SetNames(controller, "TrainerName", "TrainerInitials", trainer.name);
                }
                else if (httpUser.IsInRole("Admin"))
                {
                    var email = user.Email ?? "Administrator";
                    controller.ViewData["AdminName"] = email;
                    controller.ViewData["AdminInitials"] = "AD";
                }
            }
        }

        await next();
    }

    private static void SetNames(Controller controller, string nameKey, string initialsKey, string? fullName)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "User" : fullName;
        var initials = string.Join("",
            name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n[0])).ToUpper();

        if (initials.Length > 2) initials = initials.Substring(0, 2);

        controller.ViewData[nameKey] = name;
        controller.ViewData[initialsKey] = initials;
    }
}