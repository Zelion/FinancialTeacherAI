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
        int numberOfFacts = 4;

        try
        {
            _logger.LogInformation($"Generating Score: Question: {examDTO.Question} Student answer {examDTO.Answer}");

            // Embbed student answer
            var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(examDTO.Question);
            // Retrieve relevant chunks based on answer
            var relevantChunks = await _pineconeService.RetrieveRelevantChunksAsync(questionEmbedding.ToArray(), 3);
            var relevantChunksText = string.Join("\n", relevantChunks);

            var rightAnswer = await _promptService.GetRightAnswerAsync(examDTO.Question, relevantChunksText);
            var rightFacts = await _promptService.GetFactsAsync(numberOfFacts, rightAnswer, relevantChunksText);
            var userFacts = await _promptService.GetFactsAsync(numberOfFacts, examDTO.Answer, relevantChunksText);

            _logger.LogInformation($"Right answer: {rightAnswer}");
            _logger.LogInformation($"Right facts: {rightFacts}");
            _logger.LogInformation($"User facts: {userFacts}");

            //var score = await GetScoreAsync(examDTO.Question, examDTO.Answer, relevantChunksText);

            var scoreOnFacts = await _promptService.GetScoreOnFactsAsync(rightFacts, userFacts);

            return scoreOnFacts ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating score");
            return string.Empty;
        }
    }
}