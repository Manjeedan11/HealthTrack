using System.Collections.Generic;

namespace HealthTrack.ViewModels.Trainer
{
    public class CreateWorkoutPlanViewModel
    {
        public WorkoutPlan WorkoutPlan { get; set; } = new WorkoutPlan();
        public List<global::Client> AvailableClients { get; set; } = new List<global::Client>();
    }
}