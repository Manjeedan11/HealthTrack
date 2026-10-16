public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository feedbackRepo;

    public FeedbackService(IFeedbackRepository feedbackRepo)
    {
        this.feedbackRepo = feedbackRepo;
    }

    public async Task<bool> giveFeedback(Feedback feedback)
    {
        if (string.IsNullOrEmpty(feedback.comment))
            return false;

        feedback.date = DateTime.Now;
        await feedbackRepo.add(feedback);
        return true;
    }

    public async Task<IEnumerable<Feedback>> getClientFeedback(int clientId)
        => await feedbackRepo.getByClient(clientId);
}
