using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class DietController : Controller
{
    private readonly IDietService service;

    public DietController(IDietService service)
    {
        this.service = service;
    }

    public async Task<IActionResult> index(int clientId)
    {
        var plans = await service.getClientPlans(clientId);
        return View(plans);
    }

    [Authorize(Roles = "Trainer")]
    public IActionResult create() => View();

    [HttpPost]
    [Authorize(Roles = "Trainer")]
    public async Task<IActionResult> create(DietPlan plan)
    {
        if (!ModelState.IsValid) return View(plan);

        var success = await service.createPlan(plan);
        if (!success)
        {
            ModelState.AddModelError("", "Could not create plan.");
            return View(plan);
        }
        return RedirectToAction("index", new { clientId = plan.clientId });
    }
}