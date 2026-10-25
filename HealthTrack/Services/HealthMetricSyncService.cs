using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class HealthMetricSyncService : IHealthMetricSyncService
{
    private readonly HealthTrackDbContext context;

    public HealthMetricSyncService(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task syncTodaysCalories(int clientId)
    {
        // Define the date window for "today"
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        // 1. Sum calories consumed from today's meal logs
        var totalEaten = await context.mealLogs
            .Where(m => m.clientId == clientId
                     && m.date >= todayStart
                     && m.date < todayEnd)
            .SumAsync(m => (int?)m.caloriesConsumed) ?? 0;

        // 2. Sum calories burned from today's workout logs
        var totalBurned = await context.workoutLogs
            .Where(w => w.clientId == clientId
                     && w.date >= todayStart
                     && w.date < todayEnd)
            .SumAsync(w => (int?)w.caloriesBurned) ?? 0;

        // 3. Net intake = eaten minus burned
        var netConsumed = totalEaten - totalBurned;

        // 4. Find today's HealthMetric row (if it exists)
        var metric = await context.healthMetrics
            .FirstOrDefaultAsync(h => h.clientId == clientId
                                   && h.date >= todayStart
                                   && h.date < todayEnd);

        if (metric == null)
        {
            // 5a. No metric for today → create one
            //     Carry forward last known weight/height/bmi
            var lastMetric = await context.healthMetrics
                .Where(h => h.clientId == clientId)
                .OrderByDescending(h => h.date)
                .FirstOrDefaultAsync();

            var client = await context.clients.FindAsync(clientId);

            metric = new HealthMetric
            {
                clientId = clientId,
                date = DateTime.Now,
                weight = lastMetric?.weight ?? 0f,
                height = lastMetric?.height ?? client?.height,
                bmi = lastMetric?.bmi ?? 0f,
                caloriesBurned = totalBurned,
                caloriesConsumed = netConsumed
            };

            // If we now have weight + height but no BMI, compute it
            if (metric.bmi == 0f
                && metric.weight > 0f
                && metric.height.HasValue
                && metric.height.Value > 0)
            {
                var hMeters = metric.height.Value / 100f;
                metric.bmi = (float)Math.Round(metric.weight / (hMeters * hMeters), 2);
            }

            context.healthMetrics.Add(metric);
        }
        else
        {
            // 5b. Metric exists → update calories only
            //     NEVER overwrite weight/height/bmi here —
            //     those are owned by the HealthMetric log form
            metric.caloriesBurned = totalBurned;
            metric.caloriesConsumed = netConsumed;
        }

        // 6. Persist
        await context.SaveChangesAsync();
    }
}