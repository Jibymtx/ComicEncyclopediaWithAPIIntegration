using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IComicFilter
    {
        IEnumerable<Comic> FilterByGenre(IEnumerable<Comic> comics, string genre);
        IEnumerable<Comic> FilterByAuthor(IEnumerable<Comic> comics, string author);
        IEnumerable<Comic> FilterByYear(IEnumerable<Comic> comics, int year);
        IEnumerable<Comic> FilterByTitle(IEnumerable<Comic> comics, string title);
        IEnumerable<Comic> FilterByEdition(IEnumerable<Comic> comics, string edition);
        IEnumerable<Comic> FilterByLanguage(IEnumerable<Comic> comics, string language);
        IEnumerable<Comic> FilterByNameType(IEnumerable<Comic> comics, string nameType);

        IEnumerable<string> GetAvailableGenres(IEnumerable<Comic> comics);
        IEnumerable<string> GetAvailableAuthors(IEnumerable<Comic> comics);
        IEnumerable<int> GetAvailableYears(IEnumerable<Comic> comics);
        IEnumerable<string> GetAvailableLanguages(IEnumerable<Comic> comics);
        IEnumerable<string> GetAvailableNameTypes(IEnumerable<Comic> comics);
    }
}
