using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Business.Helpers
{
    public class ComicGrouper : IComicGrouper
    {
        public IDictionary<string, IEnumerable<Comic>> GroupByAuthor(IEnumerable<Comic> comics)
        {
            return comics
                .GroupBy(c => string.IsNullOrWhiteSpace(c.Name) ? "Unknown Author" : c.Name.Trim(),
                         StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        public IDictionary<int, IEnumerable<Comic>> GroupByYear(IEnumerable<Comic> comics)
        {
            return comics
                .Where(c => c.PublicationYear.HasValue)
                .GroupBy(c => c.PublicationYear!.Value)
                .OrderByDescending(g => g.Key)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }


        public IDictionary<string, IEnumerable<Comic>> GroupByGenre(IEnumerable<Comic> comics)
        {
            var genreGroups = new Dictionary<string, List<Comic>>(StringComparer.OrdinalIgnoreCase);

            foreach (var comic in comics)
            {
                if (string.IsNullOrWhiteSpace(comic.Genre))
                {
                    if (!genreGroups.ContainsKey("Unknown"))
                        genreGroups["Unknown"] = new List<Comic>();
                    genreGroups["Unknown"].Add(comic);
                }
                else
                {
                    var genres = comic.Genre.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(g => g.Trim());

                    foreach (var genre in genres)
                    {
                        if (!genreGroups.ContainsKey(genre))
                            genreGroups[genre] = new List<Comic>();
                        genreGroups[genre].Add(comic);
                    }
                }
            }

            return genreGroups
                .OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsEnumerable());
        }
    }
}
