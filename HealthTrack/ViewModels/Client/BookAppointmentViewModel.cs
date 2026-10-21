using System.Collections.Generic;

namespace HealthTrack.ViewModels.Client
{
    public class BookAppointmentViewModel
    {
        public Appointment Appointment { get; set; } = new Appointment();
        public List<global::Trainer> AvailableTrainers { get; set; } = new List<global::Trainer>();
    }
}