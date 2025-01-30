public interface IPineconeService
{
    Task StoreEmbeddingsAsync(List<ChunkEmbedding> chunkEmbeddings);
    Task<List<string>> RetrieveRelevantChunksAsync(string text, int topK);
    Task GenerateContextEmbeddingAsync();
    Task DeleteAllAsync();
}