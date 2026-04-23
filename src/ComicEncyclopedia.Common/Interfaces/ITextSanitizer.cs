namespace ComicEncyclopedia.Common.Interfaces
{
    public interface ITextSanitizer
    {
        string SanitizeText(string input);
        string FormatSemicolonValues(string input, string fieldName);
        IEnumerable<string> ParseMultipleValues(string input);
        string FormatISBN(string input);
    }
}
