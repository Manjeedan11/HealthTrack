public class WorkoutLog
{
    public int workoutLogId { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }
    public int? exerciseId { get; set; }
    public Exercise? exercise { get; set; }
    public int? workoutPlanId { get; set; }
    public WorkoutPlan? workoutPlan { get; set; }
    public DateTime date { get; set; }
    public int durationMinutes { get; set; }
    public int caloriesBurned { get; set; }
    public string? notes { get; set; }
}