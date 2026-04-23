using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Enums;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Business.Services
{
    public class ComicService : IComicService
    {
        private readonly IComicRepository _repository;
        private readonly IComicFilter _filter;
        private readonly IComicSorter _sorter;
        private readonly IComicGrouper _grouper;
        private readonly ITextSanitizer _textSanitizer;

        public ComicService(
            IComicRepository repository,
            IComicFilter filter,
            IComicSorter sorter,
            IComicGrouper grouper,
            ITextSanitizer textSanitizer)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _sorter = sorter ?? throw new ArgumentNullException(nameof(sorter));
            _grouper = grouper ?? throw new ArgumentNullException(nameof(grouper));
            _textSanitizer = textSanitizer ?? throw new ArgumentNullException(nameof(textSanitizer));
        }

        public bool IsDataLoaded => _repository.IsLoaded;
        public int TotalComicsCount => _repository.Count;
        public int FilteredComicsCount => _repository.FilteredCount;
        public DateTime? LastLoadedDate => _repository.LastLoadedDate;

        public async Task<int> LoadDataAsync(string filePath)
        {
            await _repository.LoadAllComicsAsync();
            return _repository.FilteredCount;
        }

        public async Task<int> LoadDataFromStreamAsync(Stream stream)
        {
            if (_repository is Data.Repositories.ComicRepository concreteRepo)
            {
                await concreteRepo.LoadFromStreamAsync(stream);
            }
            return _repository.FilteredCount;
        }


        // get all comics
        public IEnumerable<Comic> GetAllComics()
        {
            return _repository.GetFilteredComics();
        }

        public Comic? GetComicById(string blRecordId)
        {
            return _repository.GetByBLRecordId(blRecordId);
        }

        public ComicDto? GetComicDtoById(string blRecordId)
        {
            var comic = _repository.GetByBLRecordId(blRecordId);
            return comic != null ? MapToDto(comic) : null;
        }

        public IEnumerable<Comic> SearchComics(SearchQuery query)
        {
            IEnumerable<Comic> results = _repository.GetFilteredComics();

            if (!string.IsNullOrWhiteSpace(query.QueryText))
            {
                results = _filter.FilterByTitle(results, query.QueryText);
            }

            if (!string.IsNullOrWhiteSpace(query.Genre))
            {
                results = _filter.FilterByGenre(results, query.Genre);
            }

            if (!string.IsNullOrWhiteSpace(query.Author))
            {
                results = _filter.FilterByAuthor(results, query.Author);
            }

            if (query.Year.HasValue)
            {
                results = _filter.FilterByYear(results, query.Year.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Edition))
            {
                results = _filter.FilterByEdition(results, query.Edition);
            }

            if (!string.IsNullOrWhiteSpace(query.Language))
            {
                results = _filter.FilterByLanguage(results, query.Language);
            }

            if (!string.IsNullOrWhiteSpace(query.NameType))
            {
                results = _filter.FilterByNameType(results, query.NameType);
            }

            return results;
        }

        public IEnumerable<Comic> SortComics(IEnumerable<Comic> comics, SortOrder sortOrder)
        {
            return sortOrder switch
            {
                SortOrder.TitleAscending => _sorter.SortByTitleAscending(comics),
                SortOrder.TitleDescending => _sorter.SortByTitleDescending(comics),
                SortOrder.YearNewest => _sorter.SortByYearDescending(comics),
                SortOrder.YearOldest => _sorter.SortByYearAscending(comics),
                SortOrder.AuthorAscending => _sorter.SortByAuthorAscending(comics),
                SortOrder.AuthorDescending => _sorter.SortByAuthorDescending(comics),
                _ => comics
            };
        }

        public IDictionary<string, IEnumerable<Comic>> GroupComicsByAuthor(IEnumerable<Comic> comics)
        {
            return _grouper.GroupByAuthor(comics);
        }
        public IDictionary<int, IEnumerable<Comic>> GroupComicsByYear(IEnumerable<Comic> comics)
        {
            return _grouper.GroupByYear(comics);
        }

        public IDictionary<string, IEnumerable<Comic>> GroupComicsByGenre(IEnumerable<Comic> comics)
        {
            return _grouper.GroupByGenre(comics);
        }

        public IEnumerable<string> GetAvailableGenres()
        {
            return _filter.GetAvailableGenres(_repository.GetFilteredComics());
        }

        public IEnumerable<string> GetAvailableAuthors()
        {
            return _filter.GetAvailableAuthors(_repository.GetFilteredComics());
        }

        public IEnumerable<int> GetAvailableYears()
        {
            return _filter.GetAvailableYears(_repository.GetFilteredComics());
        }

        public IEnumerable<string> GetAvailableLanguages()
        {
            return _filter.GetAvailableLanguages(_repository.GetFilteredComics());
        }

        public IEnumerable<string> GetAvailableNameTypes()
        {
            return _filter.GetAvailableNameTypes(_repository.GetFilteredComics());
        }

        public string FormatFieldForDisplay(string fieldValue, string fieldName)
        {
            return _textSanitizer.FormatSemicolonValues(fieldValue, fieldName);
        }


        public ComicDto MapToDto(Comic comic)
        {
            return new ComicDto
            {
                BLRecordID = comic.BLRecordID,
                Title = comic.Title,
                Author = comic.Author,
                DateOfPublication = comic.DateOfPublication,
                PublicationYear = comic.PublicationYear,
                DisplayISBN = comic.DisplayISBN,
                Genre = comic.Genre,
                Publisher = comic.Publisher,
                Languages = comic.Languages,
                Edition = comic.Edition,
                PhysicalDescription = comic.PhysicalDescription,
                SeriesTitle = comic.SeriesTitle,
                Topics = comic.Topics,
                PlaceOfPublication = comic.PlaceOfPublication,
                AllNames = comic.AllNames,
                IsFlagged = comic.IsFlagged,
                FlagReason = comic.FlagReason,
                VariantISBNs = comic.VariantISBNs,
                VariantTitles = comic.VariantTitlesList,
                VariantYears = comic.VariantYears
            };
        }

        public IEnumerable<ComicDto> MapToDtos(IEnumerable<Comic> comics)
        {
            return comics.Select(MapToDto);
        }

        public void ClearData()
        {
            _repository.ClearAll();
        }

        public async Task ReloadDataAsync()
        {
            await _repository.ReloadDatasetAsync();
        }
    }
}
