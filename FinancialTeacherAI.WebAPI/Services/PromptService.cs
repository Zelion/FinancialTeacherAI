using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class PromptService : IPromptService
{
    private readonly Kernel _kernel;
    private readonly ILogger _logger;
    private readonly KernelPlugin _prompts;

    public PromptService(
        Kernel kernel,
        ILogger<PromptService> logger
        )
    {
        _kernel = kernel;
        _logger = logger;

        _prompts = _kernel.ImportPluginFromPromptDirectory(@$"{Directory.GetCurrentDirectory()}\Prompts");
    }

    [KernelFunction,
    Description("Recover the correct answer to a question based on the relevant chunks.")]
    public async Task<string> GetRightAnswerAsync(
        string question, 
        string relevantChunks)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetRightAnswer"],
                new()
                {
                    { "question", question },
                    { "relevant_chunks", relevantChunks }
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

    [KernelFunction,
    Description("Generate the facts from the right answer based on the relevant chunks.")]
    public async Task<string> GetFactsAsync(string statement, string relevantChunks)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetFacts"],
                new()
                {
                    { "statement", statement },
                    { "relevant_Chunks", relevantChunks }
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
                    { "relevant_chunks", relevantChunks }
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

    [KernelFunction,
    Description("Generate score based on the facts.")]
    public async Task<string> GetScoreOnFactsAsync(string rightFacts, string answer)
    {
        try
        {
            var result = await _kernel.InvokeAsync<string>(
                _prompts["GetScoreOnFacts"],
                new()
                {
                    { "rightFacts", rightFacts },
                    { "answer", answer }
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