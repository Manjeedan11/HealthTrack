public class MealLog
{
    public int mealLogId { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }
    public int? mealId { get; set; }
    public Meal? meal { get; set; }
    public int? dietPlanId { get; set; }
    public DietPlan? dietPlan { get; set; }
    public DateTime date { get; set; }
    public int caloriesConsumed { get; set; }
    public string? notes { get; set; }
}