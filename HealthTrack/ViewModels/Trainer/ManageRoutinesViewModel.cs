using System.Collections.Generic;

namespace HealthTrack.ViewModels.Trainer
{
    public class ManageRoutinesViewModel
    {
        public List<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
        public List<DietPlan> DietPlans { get; set; } = new List<DietPlan>();
    }
}