public interface IFinancialAIService
{
    Task<string> GetScoreAsync(ExamDTO examDTO);
}