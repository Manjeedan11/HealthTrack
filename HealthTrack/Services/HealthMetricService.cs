public class HealthMetricService : IHealthMetricService
{
    private readonly IHealthMetricRepository metricRepo;

    public HealthMetricService(IHealthMetricRepository metricRepo)
    {
        this.metricRepo = metricRepo;
    }

    public async Task<bool> logMetric(HealthMetric metric)
    {
        if (metric.weight <= 0)
            return false;

        metric.date = DateTime.Now;
        await metricRepo.add(metric);
        return true;
    }

    public async Task<IEnumerable<HealthMetric>> getClientMetrics(int clientId)
        => await metricRepo.getByClient(clientId);

    public Task<float> calculateBMI(float weight, float? height)
    {
        if (height == null || height <= 0)
        return Task.FromResult(0f);

        float heightInMeters = height.Value / 100;
        float bmi = weight / (heightInMeters * heightInMeters);
        return Task.FromResult((float)Math.Round(bmi, 2));
    }

}