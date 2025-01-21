public interface IFinancialAIService
{
    Task<string> GenerateScoreAsync(ExamDTO examDTO);
}