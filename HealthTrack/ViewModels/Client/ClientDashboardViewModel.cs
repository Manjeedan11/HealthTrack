using System;
using System.Collections.Generic;

namespace HealthTrack.ViewModels.Client
{
    public class ClientDashboardViewModel
    {
        
        public string ClientName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public bool IsDemonstrationData { get; set; } = false;

        
        public float? BodyWeight { get; set; }
        public string WeightUnit { get; set; } = "kg";
        public float? WeightTrend { get; set; }
        public float? Bmi { get; set; }
        public string BmiUnit { get; set; } = "";
        public string? BmiCategory { get; set; }
        public int? CaloriesBurned { get; set; }
        public int? CaloriesGoal { get; set; }

        
        public bool HasWorkout { get; set; }
        public string? WorkoutTitle { get; set; }
        public string? WorkoutSchedule { get; set; }
        public string? WorkoutTrainer { get; set; }

        
        public bool HasAppointment { get; set; }
        public string? AppointmentTitle { get; set; }
        public string? AppointmentStatus { get; set; }
        public string? AppointmentProvider { get; set; }
        public string? AppointmentSchedule { get; set; }

        
        public bool HasDietPlan { get; set; }
        public string? DietPlanName { get; set; }
        public List<MealItem> Meals { get; set; } = new List<MealItem>();

        
        public bool HasProgress { get; set; }
        public string? ProgressSummary { get; set; }
        public string? ProgressRange { get; set; }
        public List<ProgressPoint> ProgressPoints { get; set; } = new List<ProgressPoint>();

        
        public List<ReminderItem> Reminders { get; set; } = new List<ReminderItem>();

        
        public bool HasFeedback { get; set; }
        public string? FeedbackComment { get; set; }
        public string? FeedbackMeta { get; set; }
    }

    
    public class MealItem
    {
        public string MealType { get; set; } = string.Empty;
        public string MealName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string TimeOrStatus { get; set; } = string.Empty;
    }

    public class ProgressPoint
    {
        public string Label { get; set; } = string.Empty;
        public float Weight { get; set; }
    }

    public class ReminderItem
    {
        public string Title { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}