public interface IHealthMetricRepository
{
    Task<IEnumerable<HealthMetric>> getByClient(int clientId);
    Task<HealthMetric> getById(int id);
    Task add(HealthMetric metric);
    Task delete(int id);
}