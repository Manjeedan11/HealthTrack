public interface IFeedbackService
{
    Task<bool> giveFeedback(Feedback feedback);
    Task<IEnumerable<Feedback>> getClientFeedback(int clientId);
}
