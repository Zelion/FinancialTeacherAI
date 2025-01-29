using System.Diagnostics.CodeAnalysis;

public class ExamDTO
{
    [AllowNull]
    public string Question { get; set; }
    [AllowNull]
    public string Answer { get; set; }
}