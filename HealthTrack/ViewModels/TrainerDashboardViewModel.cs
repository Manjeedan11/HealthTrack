using System;
using System.Collections.Generic;

namespace HealthTrack.ViewModels
{
    public class TrainerDashboardViewModel
    {
        public string TrainerName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public bool IsDemonstrationData { get; set; } = false;
        
        public int TotalAppointments { get; set; }
        
        public List<AssignedClientItem> AssignedClients { get; set; } = new List<AssignedClientItem>();
        public List<ProgressSummaryItem> ProgressSummaries { get; set; } = new List<ProgressSummaryItem>();
        public List<UpcomingAppointmentItem> UpcomingAppointments { get; set; } = new List<UpcomingAppointmentItem>();
    }

    public class AssignedClientItem
    {
        public int ClientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string DisplayId { get; set; } = string.Empty;
        public float? LatestWeight { get; set; }
        public string WeightUnit { get; set; } = "kg";
    }

    public class ProgressSummaryItem
    {
        public string ClientFirstName { get; set; } = string.Empty;
        public float Weight { get; set; }
        public string WeightUnit { get; set; } = "kg";
        public string StatusNote { get; set; } = string.Empty;
    }

    public class UpcomingAppointmentItem
    {
        public string ClientName { get; set; } = string.Empty;
        public string TimeFormatted { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
    }
}