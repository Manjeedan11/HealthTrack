public interface IHealthMetricService
{
    Task<bool> logMetric(HealthMetric metric);
    Task<IEnumerable<HealthMetric>> getClientMetrics(int clientId);
    Task<float> calculateBMI(float weight, float? height);
}