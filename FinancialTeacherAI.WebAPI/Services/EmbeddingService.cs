using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class EmbeddingService : IEmbeddingService
{
    private readonly Kernel _kernel;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;

    public EmbeddingService(
        Kernel kernel)
    {
        _kernel = kernel;

        _textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        return await _textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
    }
}