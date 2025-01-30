using System.ComponentModel;
using Microsoft.SemanticKernel;
using Pinecone;

public class PineconeService : IPineconeService
{
    private readonly IEmbeddingService _embeddingService;

    private readonly string _pineconeApiKey;
    private readonly string _pineconeIndexName;
    private readonly string _pineconeNamespace;

    private readonly PineconeClient client;
    private readonly IndexClient index;

    public PineconeService(
        IConfiguration configuration,
        IEmbeddingService embeddingService
        )
    {
        _embeddingService = embeddingService;
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
    [KernelFunction,
    Description("Retrieves the relevant chunks from the provided text to use as context for the exam")]
    public async Task<List<string>> RetrieveRelevantChunksAsync(
        [Description("The text from where to retrieve the relevant chunks")] string text, 
        [Description("The ammount of chunks to be retrieved")] int topK
        )
    {
        var relevantChunks = new List<string>();

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(text);

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
            var chunkEmbedding = await _embeddingService.GenerateEmbeddingAsync(chunk);
            chunkEmbeddings.Add(new ChunkEmbedding
            {
                Embedding = chunkEmbedding.ToArray(),
                Text = chunk
            });
        }

        await StoreEmbeddingsAsync(chunkEmbeddings);
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