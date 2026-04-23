using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Business.Helpers
{
    public class ComicSorter : IComicSorter
    {
        public IEnumerable<Comic> SortByTitleAscending(IEnumerable<Comic> comics)
        {
            return comics.OrderBy(c => c.Title, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<Comic> SortByTitleDescending(IEnumerable<Comic> comics)
        {
            return comics.OrderByDescending(c => c.Title, StringComparer.OrdinalIgnoreCase);
        }
        public IEnumerable<Comic> SortByYearAscending(IEnumerable<Comic> comics)
        {
            return comics.OrderBy(c => c.PublicationYear ?? int.MaxValue);
        }

        public IEnumerable<Comic> SortByYearDescending(IEnumerable<Comic> comics)
        {
            return comics.OrderByDescending(c => c.PublicationYear ?? int.MinValue);
        }

        public IEnumerable<Comic> SortByAuthorAscending(IEnumerable<Comic> comics)
        {
            return comics.OrderBy(c => c.Author, StringComparer.OrdinalIgnoreCase);
        }


        public IEnumerable<Comic> SortByAuthorDescending(IEnumerable<Comic> comics)
        {
            return comics.OrderByDescending(c => c.Author, StringComparer.OrdinalIgnoreCase);
        }
    }
}
