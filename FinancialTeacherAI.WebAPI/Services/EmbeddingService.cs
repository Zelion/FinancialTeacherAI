using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class EmbeddingService : IEmbeddingService
{
    private readonly Kernel _kernel;
    private readonly IPineconeService _pineconeService;
    private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;

    public EmbeddingService(
        [FromKeyedServices("FinancialAIKernel")] Kernel kernel,
        IPineconeService pineconeService)
    {
        _kernel = kernel;
        _pineconeService = pineconeService;

        _textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    }

    [KernelFunction,
    Description("Generates the embeddings from the provided text so it can be used for similarity search")]
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        [Description("The text to generate the embeddings from")] string text
        )
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