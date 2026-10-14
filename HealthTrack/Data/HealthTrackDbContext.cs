using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class HealthTrackDbContext : IdentityDbContext<User>
{
    public HealthTrackDbContext(DbContextOptions<HealthTrackDbContext> options)
        : base(options) { }

    public DbSet<Client> clients { get; set; }
    public DbSet<Trainer> trainers { get; set; }
    public DbSet<Admin> admins { get; set; }
    public DbSet<WorkoutPlan> workoutPlans { get; set; }
    public DbSet<Exercise> exercises { get; set; }
    public DbSet<DietPlan> dietPlans { get; set; }
    public DbSet<Meal> meals { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<HealthMetric> healthMetrics { get; set; }
    public DbSet<Notification> notifications { get; set; }
    public DbSet<Feedback> feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users");
        builder.Entity<Client>().ToTable("Clients");
        builder.Entity<Trainer>().ToTable("Trainers");
        builder.Entity<Admin>().ToTable("Admins");
    }
}