public interface IEmbeddingService
{
    Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text);
    Task GenerateContextEmbeddingAsync();
}