namespace HealthTrack.Models;

public class TrainerDashboardViewModel
{
    // Trainer Identity
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = "Marcus Vance";
    public string FirstName { get; set; } = "Marcus";
    public string Initials { get; set; } = "MV";
    public string Role { get; set; } = "Trainer";
    public bool IsDemonstrationData { get; set; }
    public int UnreadNotificationCount { get; set; } = 1;

    // Assigned Clients & Progress
    public List<TrainerClientItem> AssignedClients { get; set; } = new();

    // Client Progress Summary Cards (Recent weight updates)
    public List<TrainerProgressSummaryItem> ProgressSummaries { get; set; } = new();

    // Appointments Overview
    public int TotalAppointments => UpcomingAppointments.Count;
    public List<TrainerAppointmentItem> UpcomingAppointments { get; set; } = new();
}

public class TrainerClientItem
{
    public int ClientId { get; set; }
    public string DisplayId { get; set; } = "#CL-1000";
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = "CL";
    public float? LatestWeight { get; set; }
    public string WeightUnit { get; set; } = "kg";
}

public class TrainerProgressSummaryItem
{
    public string ClientFirstName { get; set; } = string.Empty;
    public float Weight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    public string StatusNote { get; set; } = "Latest recorded";
}

public class TrainerAppointmentItem
{
    public int AppointmentId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string TimeFormatted { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // "Confirmed", "Pending", etc.
    public string ServiceType { get; set; } = "Fitness Consultation";
}
