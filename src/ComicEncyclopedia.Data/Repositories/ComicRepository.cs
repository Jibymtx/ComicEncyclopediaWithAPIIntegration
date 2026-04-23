using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Data.Repositories
{
    public class ComicRepository : IComicRepository
    {
        private readonly ICsvParser _csvParser;
        private readonly List<Comic> _allComics = new();
        private readonly List<Comic> _filteredComics = new();
        private readonly object _lock = new();
        private string _filePath = string.Empty;

        private static readonly HashSet<string> TargetGenres = new(StringComparer.OrdinalIgnoreCase)
        {
            "Fantasy",
            "Horror",
            "Science Fiction"
        };

        public ComicRepository(ICsvParser csvParser)
        {
            _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
        }

        public int Count => _allComics.Count;
        public int FilteredCount => _filteredComics.Count;
        public bool IsLoaded => _allComics.Count > 0;
        public DateTime? LastLoadedDate { get; private set; }

        public void SetFilePath(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<IEnumerable<Comic>> LoadAllComicsAsync()
        {
            // check if null
            if (string.IsNullOrWhiteSpace(_filePath))
                throw new InvalidOperationException("File path has not been set. Call SetFilePath first.");

            return await LoadFromFileAsync(_filePath);
        }


        public async Task<IEnumerable<Comic>> LoadFromFileAsync(string filePath)
        {
            _filePath = filePath;

            var loadedComics = await _csvParser.ParseFileAsync(_filePath);
            var comicsList = loadedComics.ToList();

            var mergedComics = MergeDuplicateRecords(comicsList);

            lock (_lock)
            {
                _allComics.Clear();
                _allComics.AddRange(mergedComics);

                _filteredComics.Clear();
                _filteredComics.AddRange(_allComics.Where(IsTargetGenre));

                LastLoadedDate = DateTime.UtcNow;
            }

            return _allComics.AsReadOnly();
        }

        public async Task<IEnumerable<Comic>> LoadFromStreamAsync(Stream stream)
        {
            var loadedComics = await _csvParser.ParseStreamAsync(stream);
            var comicsList = loadedComics.ToList();

            var mergedComics = MergeDuplicateRecords(comicsList);

            lock (_lock)
            {
                _allComics.Clear();
                _allComics.AddRange(mergedComics);

                _filteredComics.Clear();
                _filteredComics.AddRange(_allComics.Where(IsTargetGenre));

                LastLoadedDate = DateTime.UtcNow;
            }

            return _allComics.AsReadOnly();
        }

        // get all comics
        public IEnumerable<Comic> GetAllComics()
        {
            lock (_lock)
            {
                return _allComics.AsReadOnly();
            }
        }

        public IEnumerable<Comic> GetFilteredComics()
        {
            lock (_lock)
            {
                return _filteredComics.AsReadOnly();
            }
        }
        public Comic? GetByBLRecordId(string blRecordId)
        {
            lock (_lock)
            {
                return _allComics.FirstOrDefault(c => c.BLRecordID == blRecordId);
            }
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                _allComics.Clear();
                _filteredComics.Clear();
                LastLoadedDate = null;
            }
        }


        public async Task ReloadDatasetAsync()
        {
            if (!string.IsNullOrWhiteSpace(_filePath))
            {
                await LoadFromFileAsync(_filePath);
            }
        }

        private List<Comic> MergeDuplicateRecords(List<Comic> comics)
        {
            var grouped = comics.GroupBy(c => c.BLRecordID);
            var merged = new List<Comic>();

            foreach (var group in grouped)
            {
                var records = group.ToList();

                if (records.Count == 1)
                {
                    merged.Add(records[0]);
                    continue;
                }

                var baseRecord = records[0].Clone();

                foreach (var variant in records.Skip(1))
                {
                    if (!string.IsNullOrWhiteSpace(variant.Title) &&
                        variant.Title != baseRecord.Title &&
                        !baseRecord.VariantTitlesList.Contains(variant.Title))
                    {
                        baseRecord.VariantTitlesList.Add(variant.Title);
                    }

                    if (!string.IsNullOrWhiteSpace(variant.ISBN) &&
                        variant.ISBN != baseRecord.ISBN &&
                        variant.ISBN.ToLower() != "missing" &&
                        !baseRecord.VariantISBNs.Contains(variant.ISBN))
                    {
                        baseRecord.VariantISBNs.Add(variant.ISBN);
                    }

                    if (variant.PublicationYear.HasValue &&
                        variant.PublicationYear != baseRecord.PublicationYear &&
                        !baseRecord.VariantYears.Contains(variant.PublicationYear.Value))
                    {
                        baseRecord.VariantYears.Add(variant.PublicationYear.Value);
                    }

                    if (string.IsNullOrWhiteSpace(baseRecord.Genre) && !string.IsNullOrWhiteSpace(variant.Genre))
                        baseRecord.Genre = variant.Genre;

                    if (string.IsNullOrWhiteSpace(baseRecord.Publisher) && !string.IsNullOrWhiteSpace(variant.Publisher))
                        baseRecord.Publisher = variant.Publisher;

                    if (string.IsNullOrWhiteSpace(baseRecord.Languages) && !string.IsNullOrWhiteSpace(variant.Languages))
                        baseRecord.Languages = variant.Languages;
                }

                merged.Add(baseRecord);
            }

            return merged;
        }

        private bool IsTargetGenre(Comic comic)
        {
           if (string.IsNullOrWhiteSpace(comic.Genre))
                return false;

            var genres = comic.Genre.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(g => g.Trim());

            return genres.Any(g => TargetGenres.Any(tg =>
                g.Contains(tg, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
