public interface IDietService
{
    Task<bool> createPlan(DietPlan plan);
    Task<IEnumerable<DietPlan>> getClientPlans(int clientId);
    Task<DietPlan> getPlan(int id);
    Task<bool> deletePlan(int id);
}