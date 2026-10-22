public class Trainer {

    public int trainerId { get; set; }
    public string userId { get; set; }
    public User? user { get; set; }
    public string name { get; set; }
    public string? specialization { get; set; }
    public string? bio { get; set; }

    public ICollection<Appointment>? appointments { get; set; }
    public ICollection<WorkoutPlan>? workoutPlans { get; set; }
    public ICollection<DietPlan>? dietPlans { get; set; }
    public ICollection<Feedback>? feedbacks { get; set; } 
    
}