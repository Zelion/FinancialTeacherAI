using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class EmbeddingService : IEmbeddingService
{
    private readonly Kernel _kernel;
    private readonly IPineconeService _pineconeService;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public EmbeddingService(
        [FromKeyedServices("FinancialAIKernel")] Kernel kernel,
        IPineconeService pineconeService)
    {
        _kernel = kernel;
        _pineconeService = pineconeService;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        return await _textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
    }

    /// <summary>
    /// Get's the context and stores the embeddings in Pinecone index
    /// </summary>
    /// <returns></returns>
    public async Task GenerateContextEmbeddingAsync()
    {
        string contextPath = Path.Combine(Directory.GetCurrentDirectory(), @"data\context.txt");
        var chunks = ChunkHelper.ChunkTextByHeader(File.ReadAllText(contextPath));
        var chunkEmbeddings = new List<ChunkEmbedding>();
        foreach (string chunk in chunks)
        {
            var chunkEmbedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(chunk);
            chunkEmbeddings.Add(new ChunkEmbedding
            {
                Embedding = chunkEmbedding.ToArray(),
                Text = chunk
            });
        }

        await _pineconeService.StoreEmbeddingsAsync(chunkEmbeddings);
    }
}