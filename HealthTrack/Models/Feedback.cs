public class Feedback {

    public int feedbackId { get; set; }
    public int trainerId { get; set; }
    public Trainer? trainer { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }
    public string comment { get; set; }
    public DateTime date { get; set; }
    
}