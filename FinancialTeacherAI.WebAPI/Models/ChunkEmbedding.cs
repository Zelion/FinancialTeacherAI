using System.Diagnostics.CodeAnalysis;

public class ChunkEmbedding
{
    [AllowNull]
    public float[] Embedding { get; set; }
    [AllowNull]
    public string Text { get; set; }
}