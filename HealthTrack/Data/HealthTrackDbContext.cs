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

    
    builder.Entity<Appointment>()
        .HasOne(a => a.client)
        .WithMany(c => c.appointments)
        .HasForeignKey(a => a.clientId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<Appointment>()
        .HasOne(a => a.trainer)
        .WithMany(t => t.appointments)
        .HasForeignKey(a => a.trainerId)
        .OnDelete(DeleteBehavior.NoAction);

   
    builder.Entity<WorkoutPlan>()
        .HasOne(w => w.client)
        .WithMany(c => c.workoutPlans)
        .HasForeignKey(w => w.clientId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<WorkoutPlan>()
        .HasOne(w => w.trainer)
        .WithMany(t => t.workoutPlans)
        .HasForeignKey(w => w.trainerId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<DietPlan>()
        .HasOne(d => d.client)
        .WithMany(c => c.dietPlans)
        .HasForeignKey(d => d.clientId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<DietPlan>()
        .HasOne(d => d.trainer)
        .WithMany(t => t.dietPlans)
        .HasForeignKey(d => d.trainerId)
        .OnDelete(DeleteBehavior.NoAction);

   
    builder.Entity<Feedback>()
        .HasOne(f => f.client)
        .WithMany(c => c.feedbacks)
        .HasForeignKey(f => f.clientId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<Feedback>()
        .HasOne(f => f.trainer)
        .WithMany(t => t.feedbacks)
        .HasForeignKey(f => f.trainerId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<HealthMetric>()
        .HasOne(h => h.client)
        .WithMany(c => c.healthMetrics)
        .HasForeignKey(h => h.clientId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<HealthMetric>()
        .HasOne(h => h.exercise)
        .WithMany()
        .HasForeignKey(h => h.exerciseId)
        .OnDelete(DeleteBehavior.NoAction);

    builder.Entity<HealthMetric>()
        .HasOne(h => h.meal)
        .WithMany()
        .HasForeignKey(h => h.mealId)
        .OnDelete(DeleteBehavior.NoAction);

   
    builder.Entity<Exercise>()
        .HasOne(e => e.workoutPlan)
        .WithMany(w => w.exercises)
        .HasForeignKey(e => e.planId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Meal>()
        .HasOne(m => m.dietPlan)
        .WithMany(d => d.meals)
        .HasForeignKey(m => m.dietId)
        .OnDelete(DeleteBehavior.Cascade);
}
}