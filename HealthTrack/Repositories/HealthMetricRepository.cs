using Microsoft.EntityFrameworkCore;

public class HealthMetricRepository : IHealthMetricRepository
{
    private readonly HealthTrackDbContext context;

    public HealthMetricRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<HealthMetric>> getByClient(int clientId)
        => await context.healthMetrics
            .Where(m => m.clientId == clientId)
            .OrderByDescending(m => m.date)
            .ToListAsync();

    public async Task<HealthMetric> getById(int id)
        => await context.healthMetrics.FindAsync(id);

    public async Task add(HealthMetric metric)
    {
        context.healthMetrics.Add(metric);
        await context.SaveChangesAsync();
    }

    public async Task delete(int id)
    {
        var metric = await getById(id);
        if (metric != null)
        {
            context.healthMetrics.Remove(metric);
            await context.SaveChangesAsync();
        }
    }
}