public class FinancialAIService : IFinancialAIService
{
    private readonly ILogger _logger;
    private readonly IPineconeService _pineconeService;
    private readonly IPromptService _promptService;
    private readonly IEmbeddingService _embeddingService;

    public FinancialAIService(
        ILogger<FinancialAIService> logger,
        IPineconeService pineconeService,
        IPromptService promptService,
        IEmbeddingService embeddingService
        )
    {
        _logger = logger;
        _pineconeService = pineconeService;
        _promptService = promptService;
        _embeddingService = embeddingService;
    }

    public async Task<string> GenerateScoreAsync(ExamDTO examDTO)
    {
        //int numberOfFacts = 2;

        try
        {
            _logger.LogInformation($"Generating Score: Question: {examDTO.Question} Student answer {examDTO.Answer}");

            // Retrieve relevant chunks based on question
            var relevantChunks = await _pineconeService.RetrieveRelevantChunksAsync(examDTO.Question, 3);
            var relevantChunksText = string.Join("\n", relevantChunks);

            var rightAnswer = await _promptService.GetRightAnswerAsync(examDTO.Question, relevantChunksText);
            var rightFacts = await _promptService.GetFactsAsync(rightAnswer, relevantChunksText);

            var scoreOnFacts = await _promptService.GetScoreOnFactsAsync(rightFacts, examDTO.Answer);

            return scoreOnFacts ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating score");
            return string.Empty;
        }
    }
}