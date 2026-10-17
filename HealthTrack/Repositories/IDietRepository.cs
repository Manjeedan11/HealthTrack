public interface IDietRepository
{
    Task<IEnumerable<DietPlan>> getAll();
    Task<DietPlan?> getById(int id);
    Task<IEnumerable<DietPlan>> getByClient(int clientId);
    Task add(DietPlan plan);
    Task update(DietPlan plan);
    Task delete(int id);
}