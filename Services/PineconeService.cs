using Pinecone;

public class PineconeService : IPineconeService
{
    private readonly string _pineconeApiKey;
    private readonly string _pineconeIndexName;
    private readonly string _pineconeNamespace;

    public PineconeService(
        IConfiguration configuration
        )
    {
        _pineconeApiKey = configuration["Pinecone:ApiKey"];
        _pineconeIndexName = configuration["Pinecone:IndexName"];
        _pineconeNamespace = configuration["Pinecone:Namespace"];
    }

    /// <summary>
    /// Store embeddings in Pinecone index
    /// </summary>
    /// <param name="chunkEmbeddings"></param>
    /// <returns></returns>
    public async Task StoreEmbeddingsAsync(List<ChunkEmbedding> chunkEmbeddings)
    {
        var client = new PineconeClient(_pineconeApiKey);
        var index = client.Index(_pineconeIndexName);

        var vectorList = new List<Vector>();
        foreach (var chunkEmbedding in chunkEmbeddings)
        {
            vectorList.Add(new Vector
            {
                Id = Guid.NewGuid().ToString(),
                Values = chunkEmbedding.Embedding,
                Metadata = new Metadata
                {
                    { "text", chunkEmbedding.Text }
                }
            });
        }

        await index.UpsertAsync(new UpsertRequest
        {
            Namespace = _pineconeNamespace,
            Vectors = vectorList
        });
    }

    /// <summary>
    /// Retrieve relevant chunks from Pinecone index
    /// </summary>
    /// <param name="queryEmbedding"></param>
    /// <param name="topK"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<List<string>> RetrieveRelevantChunksAsync(float[] queryEmbedding, int topK)
    {
        var relevantChunks = new List<string>();

        var client = new PineconeClient(_pineconeApiKey);
        var index = client.Index(_pineconeIndexName);

        var queryResult = await index.QueryAsync(
            new QueryRequest
            {
                Namespace = _pineconeNamespace,
                Vector = queryEmbedding,
                TopK = (uint)topK,
                IncludeMetadata = true
            },
            new GrpcRequestOptions
            {
                MaxRetries = 3
            }
        );
        
        if (queryResult.Matches != null)
        {
            foreach (var match in queryResult.Matches)
            {
                if (match.Metadata != null && match.Metadata.Any() && match.Metadata.ContainsKey("text"))
                    relevantChunks.Add(match.Metadata["text"].ToString());
            }
        }

        return relevantChunks;
    }
}