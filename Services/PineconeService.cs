using Pinecone;

public class PineconeService : IPineconeService
{
    private readonly string _pineconeApiKey;
    private readonly string _pineconeIndexName;
    private readonly string _pineconeNamespace;

    private readonly PineconeClient client;
    private readonly IndexClient index;

    public PineconeService(
        IConfiguration configuration
        )
    {
        _pineconeApiKey = configuration["Pinecone:ApiKey"] ?? throw new ArgumentNullException("Pinecone:ApiKey");
        _pineconeIndexName = configuration["Pinecone:IndexName"] ?? throw new ArgumentNullException("Pinecone:IndexName");
        _pineconeNamespace = configuration["Pinecone:Namespace"] ?? throw new ArgumentNullException("Pinecone:Namespace");

        client = new PineconeClient(_pineconeApiKey);
        index = client.Index(_pineconeIndexName);
    }

    /// <summary>
    /// Store embeddings in Pinecone index
    /// </summary>
    /// <param name="chunkEmbeddings"></param>
    /// <returns></returns>
    public async Task StoreEmbeddingsAsync(List<ChunkEmbedding> chunkEmbeddings)
    {
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

        if (queryResult != null && queryResult.Matches != null)
        {
            foreach (var match in queryResult.Matches)
            {
                if (match != null && match.Metadata != null && match.Metadata.Any() && match.Metadata.ContainsKey("text"))
                {
                    var metadata = match.Metadata["text"];
                    if (metadata != null)
                    {
                        relevantChunks.Add(metadata.ToString());
                    }
                }
            }
        }

        return relevantChunks;
    }

    public async Task DeleteAllAsync()
    {
        await index.DeleteAsync(new DeleteRequest
        {
            Namespace = _pineconeNamespace,
            DeleteAll = true
        });
    }
}