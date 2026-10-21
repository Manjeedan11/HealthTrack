using System.Collections.Generic;

namespace HealthTrack.ViewModels.Admin
{
    public class AssignTrainerViewModel
    {
        public List<global::Client> Clients { get; set; } = new List<global::Client>();
        public List<global::Trainer> Trainers { get; set; } = new List<global::Trainer>();
    }
}