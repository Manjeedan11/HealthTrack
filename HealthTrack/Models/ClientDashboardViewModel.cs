namespace HealthTrack.Models;

public class ClientDashboardViewModel
{
    // Client identity
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Initials { get; set; } = "CL";
    public string Role { get; set; } = "Client";
    public bool IsDemonstrationData { get; set; }

    // Health Summary KPIs
    public float? BodyWeight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    public float? WeightTrend { get; set; }
    public float? Bmi { get; set; }
    public string BmiUnit { get; set; } = "kg/m²";
    public string? BmiCategory { get; set; }
    public int? CaloriesBurned { get; set; }
    public int? CaloriesGoal { get; set; }

    // Today's Workout Card
    public bool HasWorkout { get; set; }
    public string? WorkoutTitle { get; set; }
    public string? WorkoutSchedule { get; set; }
    public string? WorkoutTrainer { get; set; }

    // Upcoming Appointment Card
    public bool HasAppointment { get; set; }
    public string? AppointmentTitle { get; set; }
    public string? AppointmentStatus { get; set; }
    public string? AppointmentProvider { get; set; }
    public string? AppointmentSchedule { get; set; }

    // Today's Diet Plan Card
    public bool HasDietPlan { get; set; }
    public string? DietPlanName { get; set; }
    public List<DashboardMealItem> Meals { get; set; } = new();

    // Progress Tracking Card
    public bool HasProgress { get; set; }
    public string? ProgressSummary { get; set; }
    public string? ProgressRange { get; set; }
    public List<ProgressPoint> ProgressPoints { get; set; } = new();

    // Reminders Card
    public List<ReminderItem> Reminders { get; set; } = new();

    // Latest Trainer Feedback Card
    public bool HasFeedback { get; set; }
    public string? FeedbackComment { get; set; }
    public string? FeedbackMeta { get; set; }

    // Notification Count
    public int UnreadNotificationCount { get; set; }
}

public class DashboardMealItem
{
    public string MealType { get; set; } = string.Empty; // e.g. Breakfast, Lunch, Dinner
    public string MealName { get; set; } = string.Empty; // e.g. Oats & Berries
    public string TimeOrStatus { get; set; } = string.Empty; // e.g. Completed, 1:00 PM
    public bool IsCompleted { get; set; }
}

public class ProgressPoint
{
    public string Label { get; set; } = string.Empty; // e.g. W1, W2, Today
    public float Weight { get; set; }
}

public class ReminderItem
{
    public string Title { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}
