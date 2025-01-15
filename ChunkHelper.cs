using System.Text.RegularExpressions;
using iText.Kernel.Pdf;

public static class ChunkHelper
{
    public static List<string> ChunkPdf(PdfDocument pdfDoc, int chunkSize)
    {
        var chunks = new List<string>();
        var pagesCount = 1;
        while (pagesCount <= pdfDoc.GetNumberOfPages())
        {
            var page = pdfDoc.GetPage(pagesCount);
            var text = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page);
            var sentences = text.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
            var currentChunk = new List<string>();
            foreach (var sentence in sentences)
            {
                currentChunk.Add(sentence);
                if (currentChunk.Count >= chunkSize)
                {
                    chunks.Add(string.Join(". ", currentChunk));
                    currentChunk.Clear();
                }
            }

            if (currentChunk.Count > 0)
                chunks.Add(string.Join(". ", currentChunk));

            pagesCount++;
        }

        return chunks;
    }

    public static List<string> ChunkTextByHeader(string text)
    {
        // Regex pattern to identify headers (e.g., #, ##)
        string headerPattern = @"(?=^#.+?$)";

        // Split the text based on headers using Regex
        var sections = Regex.Split(text, headerPattern, RegexOptions.Multiline);

        // Create a list to store chunks
        var chunks = new List<string>();

        foreach (var section in sections)
        {
            if (!string.IsNullOrWhiteSpace(section))
            {
                chunks.Add(section.Trim());
            }
        }

        return chunks;
    }
}