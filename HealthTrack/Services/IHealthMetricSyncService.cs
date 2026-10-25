using System.Threading.Tasks;

public interface IHealthMetricSyncService
{
    /// <summary>
    /// Recalculates today's HealthMetric caloriesBurned and caloriesConsumed
    /// (net intake) from the client's workout and meal logs for today.
    /// Creates a HealthMetric row for today if one doesn't exist,
    /// or updates the existing one — preserving weight, height, and BMI.
    /// </summary>
    Task syncTodaysCalories(int clientId);
}