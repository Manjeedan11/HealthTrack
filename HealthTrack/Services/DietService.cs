public class DietService : IDietService
{
    private readonly IDietRepository dietRepo;
    private readonly INotificationRepository notificationRepo;

    public DietService(IDietRepository dietRepo,
                       INotificationRepository notificationRepo)
    {
        this.dietRepo = dietRepo;
        this.notificationRepo = notificationRepo;
    }

    public async Task<bool> createPlan(DietPlan plan)
    {
        if (string.IsNullOrEmpty(plan.name))
            return false;

        plan.dateCreated = DateTime.Now;
        await dietRepo.add(plan);

        var client = await dietRepo.getById(plan.dietId);
        if (client != null)
        {
            await notificationRepo.add(new Notification
            {
                userId = client.client.userId,
                message = $"New diet plan assigned: {plan.name}",
                date = DateTime.Now,
                isRead = false
            });
        }
        return true;
    }

    public async Task<IEnumerable<DietPlan>> getClientPlans(int clientId)
        => await dietRepo.getByClient(clientId);

    public async Task<DietPlan?> getPlan(int id)
        => await dietRepo.getById(id);

    public async Task<bool> deletePlan(int id)
    {
        await dietRepo.delete(id);
        return true;
    }
}