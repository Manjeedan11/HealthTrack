using Microsoft.EntityFrameworkCore;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly HealthTrackDbContext context;

    public WorkoutRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<WorkoutPlan>> getAll()
        => await context.workoutPlans.Include(p => p.exercises).ToListAsync();

    public async Task<WorkoutPlan?> getById(int id)
        => await context.workoutPlans
            .Include(p => p.exercises)
            .FirstOrDefaultAsync(p => p.planId == id);

    public async Task<IEnumerable<WorkoutPlan>> getByClient(int clientId)
        => await context.workoutPlans
            .Include(p => p.exercises)
            .Where(p => p.clientId == clientId)
            .ToListAsync();

    public async Task add(WorkoutPlan plan)
    {
        context.workoutPlans.Add(plan);
        await context.SaveChangesAsync();
    }

    public async Task update(WorkoutPlan plan)
    {
        context.workoutPlans.Update(plan);
        await context.SaveChangesAsync();
    }

    public async Task delete(int id)
    {
        var plan = await getById(id);
        if (plan != null)
        {
            context.workoutPlans.Remove(plan);
            await context.SaveChangesAsync();
        }
    }
}