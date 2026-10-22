using System.Collections.Generic;

namespace HealthTrack.ViewModels.Shared
{
    public class AppointmentIndexViewModel
    {
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CancelledCount { get; set; }
         public int CompletedCount { get; set; } 
        public bool IsTrainer { get; set; }
    }
}