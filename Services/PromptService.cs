using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class PromptService : IPromptService
{
    private readonly Kernel _kernel;
    private readonly ILogger _logger;
    private readonly KernelPlugin _prompts;

    private readonly OpenAIPromptExecutionSettings _settings;

    public PromptService(
        [FromKeyedServices("FinancialAIKernel")] Kernel kernel,
        ILogger<PromptService> logger
        )
    {
        _kernel = kernel;
        _logger = logger;

        _prompts = _kernel.ImportPluginFromPromptDirectory(@$"{Directory.GetCurrentDirectory()}\Prompts");

        _settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            ChatSystemPrompt = $@"You are finance teacher and you are reviewing student exams and giving scores based on the answers.",
        };
    }

    public async Task<string> GetRightAnswerAsync(string question, string relevantChunks)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetRightAnswer"],
                new()
                {
                    { "question", question },
                    { "relevant_chunks", relevantChunks },
                    { "settings", _settings }
                }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating right answer");
            return string.Empty;
        }
    }

    public async Task<string> GetFactsAsync(int numberOfFacts, string statement, string relevantChunks)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetFacts"],
                new()
                {
                    { "numberOfFacts", numberOfFacts },
                    { "statement", statement },
                    { "relevant_Chunks", relevantChunks },
                    { "settings", _settings }
                }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating facts");
            return string.Empty;
        }
    }

    public async Task<string> GetScoreAsync(string question, string answer, string relevantChunks)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetScore"],
                new()
                {
                    { "question", question },
                    { "answer", answer },
                    { "relevant_chunks", relevantChunks },
                    { "settings", _settings }
                }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating score");
            return string.Empty;
        }
    }

    public async Task<string> GetScoreOnFactsAsync(string rightFacts, string userFacts)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetScoreOnFacts"],
                new()
                {
                    { "rightFacts", rightFacts },
                    { "userFacts", userFacts },
                    { "settings", _settings }
                }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating score on facts");
            return string.Empty;
        }
    }
}