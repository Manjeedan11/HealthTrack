public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository workoutRepo;
    private readonly INotificationRepository notificationRepo;

    public WorkoutService(IWorkoutRepository workoutRepo,
                          INotificationRepository notificationRepo)
    {
        this.workoutRepo = workoutRepo;
        this.notificationRepo = notificationRepo;
    }

    public async Task<bool> createPlan(WorkoutPlan plan)
    {
        if (string.IsNullOrEmpty(plan.name))
            return false;

        plan.dateCreated = DateTime.Now;
        await workoutRepo.add(plan);

        var client = await workoutRepo.getById(plan.planId);
        if (client != null)
        {
            await notificationRepo.add(new Notification
            {
                userId = client.client.userId,
                message = $"New workout plan assigned: {plan.name}",
                date = DateTime.Now,
                isRead = false
            });
        }
        return true;
    }

    public async Task<IEnumerable<WorkoutPlan>> getClientPlans(int clientId)
        => await workoutRepo.getByClient(clientId);

    public async Task<WorkoutPlan> getPlan(int id)
        => await workoutRepo.getById(id);

    public async Task<bool> deletePlan(int id)
    {
        await workoutRepo.delete(id);
        return true;
    }
}
