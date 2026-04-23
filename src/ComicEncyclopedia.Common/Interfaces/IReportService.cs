using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IReportService
    {
        Task RecordSearchAsync(SearchQuery query, IEnumerable<Comic> results);
        Task<IEnumerable<SearchResultDto>> GetComicsInMoreThan100ResultsAsync();
        Task<IEnumerable<SearchQueryDto>> GetTop10QueriesAsync();
        Task<IEnumerable<SearchResultDto>> GetTop10ResultsAsync();
        Task<ReportDto> GetFullReportAsync();
        Task ClearReportDataAsync();
        Task<int> GetTotalSearchCountAsync();
    }
}
