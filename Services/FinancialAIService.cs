using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

public class FinancialAIService : IFinancialAIService
{
    private readonly Kernel _kernel;
    private readonly IPineconeService _pineconeService;

    public FinancialAIService(
        [FromKeyedServices("FinancialAIKernel")] Kernel kernel,
        IPineconeService pineconeService
        )
    {
        _kernel = kernel;
        _pineconeService = pineconeService;
    }

    public async Task<string> GetScoreAsync(ExamDTO examDTO)
    {
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var textEmbedding = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ChatSystemPrompt = $@"You are finance teacher and you are reviewing student exams and giving scores based on the answers.",
        };

        var prompts = _kernel.ImportPluginFromPromptDirectory(@$"{Directory.GetCurrentDirectory()}\Prompts");
        // await _kernel.InvokePromptAsync(@"Print the next line: 'Answer the following questions: '", new(settings));

        // Embbed student answer
        var answerEmbedding = await textEmbedding.GenerateEmbeddingAsync(examDTO.Answer);
        // Retrieve relevant chunks based on answer
        var relevantChunks = await _pineconeService.RetrieveRelevantChunksAsync(answerEmbedding.ToArray(), 3);

        var score = await _kernel.InvokeAsync<string>(
            prompts["GetScore"],
            new()
            {
                    { "question", examDTO.Question },
                    { "answer", examDTO.Answer },
                    { "relevant_chunks", string.Join("\n", relevantChunks) },
                    { "settings", settings }
            }
        );

        return score;
    }
}