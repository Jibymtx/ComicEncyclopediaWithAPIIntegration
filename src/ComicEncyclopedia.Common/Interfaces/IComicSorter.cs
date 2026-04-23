using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IComicSorter
    {
        IEnumerable<Comic> SortByTitleAscending(IEnumerable<Comic> comics);
        IEnumerable<Comic> SortByTitleDescending(IEnumerable<Comic> comics);
        IEnumerable<Comic> SortByYearAscending(IEnumerable<Comic> comics);
        IEnumerable<Comic> SortByYearDescending(IEnumerable<Comic> comics);
        IEnumerable<Comic> SortByAuthorAscending(IEnumerable<Comic> comics);
        IEnumerable<Comic> SortByAuthorDescending(IEnumerable<Comic> comics);
    }
}
