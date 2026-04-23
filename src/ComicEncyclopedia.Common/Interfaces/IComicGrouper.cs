using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IComicGrouper
    {
        IDictionary<string, IEnumerable<Comic>> GroupByAuthor(IEnumerable<Comic> comics);
        IDictionary<int, IEnumerable<Comic>> GroupByYear(IEnumerable<Comic> comics);
        IDictionary<string, IEnumerable<Comic>> GroupByGenre(IEnumerable<Comic> comics);
    }
}
