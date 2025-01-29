public interface IPromptService
{
    Task<string> GetRightAnswerAsync(string question, string relevantChunks);
    Task<string> GetFactsAsync(string statement, string relevantChunks);
    Task<string> GetScoreAsync(string question, string answer, string relevantChunks);
    Task<string> GetScoreOnFactsAsync(string rightFacts, string userFacts);
}