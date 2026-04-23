using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface ICsvParser
    {
        Task<IEnumerable<Comic>> ParseFileAsync(string filePath);
        Comic? ParseLine(string line, string[] headers);
        Task<IEnumerable<Comic>> ParseStreamAsync(Stream stream);
    }
}
