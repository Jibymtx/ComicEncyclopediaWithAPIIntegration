using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Business.Helpers
{
    public class ComicFilter : IComicFilter
    {
        private static readonly HashSet<string> TargetGenres = new(StringComparer.OrdinalIgnoreCase)
        {
            "Fantasy",
            "Horror",
            "Science Fiction"
        };

        public IEnumerable<Comic> FilterByGenre(IEnumerable<Comic> comics, string genre)
        {
            if (string.IsNullOrWhiteSpace(genre))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.Genre) &&
                c.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Comic> FilterByAuthor(IEnumerable<Comic> comics, string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.Name) &&
                c.Name.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Comic> FilterByYear(IEnumerable<Comic> comics, int year)
        {
            return comics.Where(c => c.PublicationYear == year);
        }
        public IEnumerable<Comic> FilterByTitle(IEnumerable<Comic> comics, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.Title) &&
                c.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Comic> FilterByEdition(IEnumerable<Comic> comics, string edition)
        {
            if (string.IsNullOrWhiteSpace(edition))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.Edition) &&
                c.Edition.Contains(edition, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Comic> FilterByLanguage(IEnumerable<Comic> comics, string language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.Languages) &&
                c.Languages.Contains(language, StringComparison.OrdinalIgnoreCase));
        }


        public IEnumerable<Comic> FilterByNameType(IEnumerable<Comic> comics, string nameType)
        {
            if (string.IsNullOrWhiteSpace(nameType))
                return comics;

            return comics.Where(c =>
                !string.IsNullOrWhiteSpace(c.TypeOfName) &&
                c.TypeOfName.Contains(nameType, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetAvailableGenres(IEnumerable<Comic> comics)
        {
            var allGenres = comics
                .Where(c => !string.IsNullOrWhiteSpace(c.Genre))
                .SelectMany(c => c.Genre.Split(';', StringSplitOptions.RemoveEmptyEntries))
                .Select(g => g.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return allGenres
                .Where(g => TargetGenres.Any(tg => g.Contains(tg, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(g => g)
                .Distinct();
        }

        public IEnumerable<string> GetAvailableAuthors(IEnumerable<Comic> comics)
        {
            return comics
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Select(c => c.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a);
        }

        public IEnumerable<int> GetAvailableYears(IEnumerable<Comic> comics)
        {
            return comics
                .Where(c => c.PublicationYear.HasValue)
                .Select(c => c.PublicationYear!.Value)
                .Distinct()
                .OrderByDescending(y => y);
        }

        public IEnumerable<string> GetAvailableLanguages(IEnumerable<Comic> comics)
        {
            return comics
                .Where(c => !string.IsNullOrWhiteSpace(c.Languages))
                .SelectMany(c => c.Languages.Split(';', StringSplitOptions.RemoveEmptyEntries))
                .Select(l => l.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(l => l);
        }

        public IEnumerable<string> GetAvailableNameTypes(IEnumerable<Comic> comics)
        {
            return comics
                .Where(c => !string.IsNullOrWhiteSpace(c.TypeOfName))
                .Select(c => c.TypeOfName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n);
        }
    }
}
