using Microsoft.EntityFrameworkCore;

public class DietRepository : IDietRepository
{
    private readonly HealthTrackDbContext context;

    public DietRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<DietPlan>> getAll()
        => await context.dietPlans.Include(p => p.meals).ToListAsync();

    public async Task<DietPlan> getById(int id)
        => await context.dietPlans
            .Include(p => p.meals)
            .FirstOrDefaultAsync(p => p.dietId == id);

    public async Task<IEnumerable<DietPlan>> getByClient(int clientId)
        => await context.dietPlans
            .Include(p => p.meals)
            .Where(p => p.clientId == clientId)
            .ToListAsync();

    public async Task add(DietPlan plan)
    {
        context.dietPlans.Add(plan);
        await context.SaveChangesAsync();
    }

    public async Task update(DietPlan plan)
    {
        context.dietPlans.Update(plan);
        await context.SaveChangesAsync();
    }

    public async Task delete(int id)
    {
        var plan = await getById(id);
        if (plan != null)
        {
            context.dietPlans.Remove(plan);
            await context.SaveChangesAsync();
        }
    }
}