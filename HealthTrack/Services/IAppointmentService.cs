public interface IAppointmentService
{
    Task<bool> bookAppointment(Appointment appointment);
    Task<IEnumerable<Appointment>> getClientAppointments(int clientId);
}