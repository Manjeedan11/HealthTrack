using System.ComponentModel.DataAnnotations;

public class DietPlan
{
    [Key]
    public int dietId { get; set; }
    public int trainerId { get; set; }
    public Trainer? trainer { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public DateTime dateCreated { get; set; }

    public ICollection<Meal>? meals { get; set; }
}