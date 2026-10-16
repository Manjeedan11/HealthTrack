public interface IWorkoutRepository
{
    Task<IEnumerable<WorkoutPlan>> getAll();
    Task<WorkoutPlan> getById(int id);
    Task<IEnumerable<WorkoutPlan>> getByClient(int clientId);
    Task add(WorkoutPlan plan);
    Task update(WorkoutPlan plan);
    Task delete(int id);
}