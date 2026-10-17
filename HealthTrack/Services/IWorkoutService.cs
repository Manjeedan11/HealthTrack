public interface IWorkoutService
{
    Task<bool> createPlan(WorkoutPlan plan);
    Task<IEnumerable<WorkoutPlan>> getClientPlans(int clientId);
    Task<WorkoutPlan?> getPlan(int id);
    Task<bool> deletePlan(int id);
}
