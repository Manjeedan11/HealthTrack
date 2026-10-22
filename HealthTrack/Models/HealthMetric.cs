using System.ComponentModel.DataAnnotations;

public class HealthMetric
{
    [Key]
    public int metricId { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }
    public int? exerciseId { get; set; }
    public Exercise? exercise { get; set; }
    public int? mealId { get; set; }
    public Meal? meal { get; set; }
    public DateTime date { get; set; }
    public float weight { get; set; }
    public float bmi { get; set; }
    public int caloriesBurned { get; set; }
    public int caloriesConsumed { get; set; }

}