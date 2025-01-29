using System.Diagnostics.CodeAnalysis;

public class Answer
{
    public int Id { get; set; }
    [AllowNull]
    public string Text { get; set; }
}