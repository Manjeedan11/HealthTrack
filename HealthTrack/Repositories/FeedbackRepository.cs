using Microsoft.EntityFrameworkCore;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly HealthTrackDbContext context;

    public FeedbackRepository(HealthTrackDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Feedback>> getByClient(int clientId)
        => await context.feedbacks
            .Where(f => f.clientId == clientId)
            .OrderByDescending(f => f.date)
            .ToListAsync();

    public async Task<IEnumerable<Feedback>> getByTrainer(int trainerId)
        => await context.feedbacks
            .Where(f => f.trainerId == trainerId)
            .OrderByDescending(f => f.date)
            .ToListAsync();

    public async Task add(Feedback feedback)
    {
        context.feedbacks.Add(feedback);
        await context.SaveChangesAsync();
    }
}