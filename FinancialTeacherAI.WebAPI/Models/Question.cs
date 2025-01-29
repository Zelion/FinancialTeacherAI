using System.Diagnostics.CodeAnalysis;

public class Question 
{
    public int Id { get; set; }
    [AllowNull]
    public string Text { get; set; }
}