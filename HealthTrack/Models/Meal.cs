public class Meal {

    public int mealId { get; set; }
    public int dietId { get; set; }
    public DietPlan? dietPlan { get; set; }
    public string name { get; set; }
    public int calories { get; set; }
    public string mealTime { get; set; }
    
}