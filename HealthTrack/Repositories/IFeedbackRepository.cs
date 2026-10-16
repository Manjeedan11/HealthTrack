public interface IFeedbackRepository
{
    Task<IEnumerable<Feedback>> getByClient(int clientId);
    Task<IEnumerable<Feedback>> getByTrainer(int trainerId);
    Task add(Feedback feedback);
}