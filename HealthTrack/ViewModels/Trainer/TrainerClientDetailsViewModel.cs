using System.Collections.Generic;

namespace HealthTrack.ViewModels.Trainer
{
    public class TrainerClientDetailsViewModel
    {
       
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
        public string Initials { get; set; } = "";
        public string DisplayId { get; set; } = "";
        public string? Gender { get; set; }
        public float? Height { get; set; }
        public DateTime? DateOfBirth { get; set; }

        
        public float? LatestWeight { get; set; }
        public float? LatestBmi { get; set; }
        public int? LatestCaloriesBurned { get; set; }
        public int? LatestCaloriesConsumed { get; set; }
        public DateTime? LastMetricDate { get; set; }

       
        public int TotalMetrics { get; set; }
        public int TotalWorkoutLogs { get; set; }
        public int TotalMealLogs { get; set; }

       
        public List<HealthMetric> RecentMetrics { get; set; } = new();
        public List<WorkoutLog> RecentWorkoutLogs { get; set; } = new();
        public List<MealLog> RecentMealLogs { get; set; } = new();
    }
}