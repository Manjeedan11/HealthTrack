using System.ComponentModel.DataAnnotations;

public class WorkoutPlan{

    [Key]
    public int planId { get; set; }
    public int trainerId { get; set; }
    public Trainer trainer { get; set; }
    public int clientId { get; set; }
    public Client client { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime dateCreated { get; set; }

    public ICollection<Exercise> exercises { get; set; }

}