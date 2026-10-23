using System;
using System.Collections.Generic;

namespace HealthTrack.ViewModels.Trainer
{
    public class TrainerReportsViewModel
    {
       
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string ReportType { get; set; } = "all";

        
        public int TotalWorkoutLogs { get; set; }
        public int TotalMealLogs { get; set; }
        public int TotalMetrics { get; set; }
        public int TotalEngagement { get; set; }

        
        public List<ClientPerformanceRow> ClientPerformance { get; set; } = new();
        public List<RoutineUsageItem> PopularWorkoutPlans { get; set; } = new();
        public List<RoutineUsageItem> PopularDietPlans { get; set; } = new();
        public List<EngagementDayItem> EngagementByDay { get; set; } = new();
    }

    public class ClientPerformanceRow
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
        public string Initials { get; set; } = "";
        public int WorkoutLogs { get; set; }
        public int MealLogs { get; set; }
        public int HealthMetrics { get; set; }
        public int TotalActivity { get; set; }
    }

    public class RoutineUsageItem
    {
        public string Name { get; set; } = "";
        public int UsageCount { get; set; }
        public string Color { get; set; } = "#0d6e5a";
    }

    public class EngagementDayItem
    {
        public DateTime Date { get; set; }
        public int WorkoutLogs { get; set; }
        public int MealLogs { get; set; }
        public int Metrics { get; set; }
        public int Total { get; set; }
    }
}