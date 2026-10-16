public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> getAll();
    Task<Appointment> getById(int id);
    Task add(Appointment appointment);
    Task update(Appointment appointment);
    Task delete(int id);
}