public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository appointmentRepo;

    public AppointmentService(IAppointmentRepository appointmentRepo)
    {
        this.appointmentRepo = appointmentRepo;
    }

    public async Task<bool> bookAppointment(Appointment appointment)
    {
        if (appointment.dateTime <= DateTime.Now)
            return false;

        var existing = await appointmentRepo.getAll();
        bool conflict = existing.Any(a =>
            a.trainerId == appointment.trainerId &&
            a.dateTime == appointment.dateTime &&
            a.status != "Cancelled");

        if (conflict) return false;

        appointment.status = "Pending";
        await appointmentRepo.add(appointment);
        return true;
    }

    public async Task<IEnumerable<Appointment>> getClientAppointments(int clientId)
    {
        var all = await appointmentRepo.getAll();
        return all.Where(a => a.clientId == clientId).ToList();
    }
}