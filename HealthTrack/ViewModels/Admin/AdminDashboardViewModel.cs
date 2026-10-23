using System;
using System.Collections.Generic;

namespace HealthTrack.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Admin identity
        public string AdminName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string Initials { get; set; } = "";
        public bool IsDemonstrationData { get; set; } = false;

        // KPI counts
        public int TotalUsers { get; set; }
        public int TotalClients { get; set; }
        public int TotalTrainers { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalWorkoutPlans { get; set; }
        public int TotalDietPlans { get; set; }

        // Lists
        public List<AdminRecentUser> RecentUsers { get; set; } = new();
        public List<AdminRecentAppointment> RecentAppointments { get; set; } = new();
    }

    public class AdminRecentUser
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string Initials { get; set; } = "";
        public string DisplayId { get; set; } = "";
    }

    public class AdminRecentAppointment
    {
        public string ClientName { get; set; } = "";
        public string TrainerName { get; set; } = "";
        public string TimeFormatted { get; set; } = "";
        public string Status { get; set; } = "";
        public string ServiceType { get; set; } = "Consultation";
    }
}