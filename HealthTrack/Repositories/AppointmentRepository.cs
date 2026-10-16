using Microsoft.EntityFrameworkCore;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly HealthTrackDbContext context;

    public AppointmentRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Appointment>> getAll()
        => await context.appointments.ToListAsync();

    public async Task<Appointment> getById(int id)
        => await context.appointments.FindAsync(id);

    public async Task add(Appointment appointment)
    {
        context.appointments.Add(appointment);
        await context.SaveChangesAsync();
    }

    public async Task update(Appointment appointment)
    {
        context.appointments.Update(appointment);
        await context.SaveChangesAsync();
    }

    public async Task delete(int id)
    {
        var appointment = await getById(id);
        if (appointment != null)
        {
            context.appointments.Remove(appointment);
            await context.SaveChangesAsync();
        }
    }
}