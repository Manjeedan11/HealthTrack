using System.Collections.Generic;

namespace HealthTrack.ViewModels.Trainer
{
    public class CreateDietPlanViewModel
    {
        public DietPlan DietPlan { get; set; } = new DietPlan();
        public List<global::Client> AvailableClients { get; set; } = new List<global::Client>();
    }
}