public class Client
{
    public int clientId { get; set; }
    public string userId { get; set; }
    public User user { get; set; }
    public string name { get; set; }
    public DateTime? dateOfBirth { get; set; }
    public string? gender { get; set; }
    public float? height { get; set; }

    public int? trainerId { get; set; }
    public Trainer? trainer { get; set; }

    public ICollection<HealthMetric> healthMetrics { get; set; }
    public ICollection<Appointment> appointments { get; set; }
    public ICollection<WorkoutPlan> workoutPlans { get; set; }
    public ICollection<DietPlan> dietPlans { get; set; }
    public ICollection<Feedback> feedbacks { get; set; }
}