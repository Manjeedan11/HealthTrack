public class Appointment
{
    public int appointmentId { get; set; }
    public int clientId { get; set; }
    public Client? client { get; set; }      
    public int trainerId { get; set; }
    public Trainer? trainer { get; set; }     
    public DateTime dateTime { get; set; }
    public string status { get; set; } = "Pending";
    public string notes { get; set; } = "";
}