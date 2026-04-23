using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Enums;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Business.Services
{
    public interface IComicService
    {
        bool IsDataLoaded { get; }
        int TotalComicsCount { get; }
        int FilteredComicsCount { get; }
        DateTime? LastLoadedDate { get; }

        Task<int> LoadDataAsync(string filePath);
        Task<int> LoadDataFromStreamAsync(Stream stream);
        IEnumerable<Comic> GetAllComics();
        Comic? GetComicById(string blRecordId);
        ComicDto? GetComicDtoById(string blRecordId);
        IEnumerable<Comic> SearchComics(SearchQuery query);
        IEnumerable<Comic> SortComics(IEnumerable<Comic> comics, SortOrder sortOrder);
        IDictionary<string, IEnumerable<Comic>> GroupComicsByAuthor(IEnumerable<Comic> comics);
        IDictionary<int, IEnumerable<Comic>> GroupComicsByYear(IEnumerable<Comic> comics);
        IDictionary<string, IEnumerable<Comic>> GroupComicsByGenre(IEnumerable<Comic> comics);
        IEnumerable<string> GetAvailableGenres();
        IEnumerable<string> GetAvailableAuthors();
        IEnumerable<int> GetAvailableYears();
        IEnumerable<string> GetAvailableLanguages();
        IEnumerable<string> GetAvailableNameTypes();
        string FormatFieldForDisplay(string fieldValue, string fieldName);
        ComicDto MapToDto(Comic comic);
        IEnumerable<ComicDto> MapToDtos(IEnumerable<Comic> comics);
        void ClearData();
        Task ReloadDataAsync();
    }
}
