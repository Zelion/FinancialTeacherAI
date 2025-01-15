public interface IPineconeService
{
    Task StoreEmbeddingsAsync(List<ChunkEmbedding> chunkEmbeddings);
    Task<List<string>> RetrieveRelevantChunksAsync(float[] queryEmbedding, int topK);
}