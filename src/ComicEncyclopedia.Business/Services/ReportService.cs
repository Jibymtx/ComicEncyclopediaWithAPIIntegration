using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using ComicEncyclopedia.Data.Context;
using ComicEncyclopedia.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComicEncyclopedia.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task RecordSearchAsync(SearchQuery query, IEnumerable<Comic> results)
        {
            // check if null
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var resultsList = results.ToList();

            var searchLog = new SearchLog
            {
                UserId = query.UserId,
                QueryText = query.QueryText ?? string.Empty,
                Genre = query.Genre,
                Author = query.Author,
                Year = query.Year,
                Edition = query.Edition,
                Language = query.Language,
                NameType = query.NameType,
                ResultCount = resultsList.Count,
                SearchTime = DateTime.UtcNow,
                QueryHash = GenerateQueryHash(query)
            };

            _context.SearchLogs.Add(searchLog);

            foreach (var comic in resultsList)
            {
                var existingResult = await _context.ResultLogs
                    .FirstOrDefaultAsync(r => r.BLRecordID == comic.BLRecordID);

                if (existingResult != null)
                {
                    existingResult.TimesReturned++;
                    existingResult.LastReturnedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.ResultLogs.Add(new ResultLog
                    {
                        BLRecordID = comic.BLRecordID,
                        Title = comic.Title,
                        Author = comic.Author,
                        TimesReturned = 1,
                        FirstReturnedDate = DateTime.UtcNow,
                        LastReturnedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<SearchResultDto>> GetComicsInMoreThan100ResultsAsync()
        {
            return await _context.ResultLogs
                .Where(r => r.TimesReturned > 100)
                .OrderByDescending(r => r.TimesReturned)
                .Select(r => new SearchResultDto
                {
                    ComicTitle = r.Title,
                    ComicBLRecordID = r.BLRecordID,
                    Author = r.Author,
                    TimesReturned = r.TimesReturned,
                    LastReturnedDate = r.LastReturnedDate
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<SearchQueryDto>> GetTop10QueriesAsync()
        {
            return await _context.SearchLogs
                .GroupBy(s => s.QueryHash)
                .Select(g => new SearchQueryDto
                {
                    QueryDescription = g.First().QueryText ?? "All Comics",
                    TimesSearched = g.Count(),
                    TotalResultsReturned = g.Sum(s => s.ResultCount),
                    LastSearchedDate = g.Max(s => s.SearchTime)
                })
                .OrderByDescending(q => q.TimesSearched)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<SearchResultDto>> GetTop10ResultsAsync()
        {
            return await _context.ResultLogs
                .OrderByDescending(r => r.TimesReturned)
                .Take(10)
                .Select(r => new SearchResultDto
                {
                    ComicTitle = r.Title,
                    ComicBLRecordID = r.BLRecordID,
                    Author = r.Author,
                    TimesReturned = r.TimesReturned,
                    LastReturnedDate = r.LastReturnedDate
                })
                .ToListAsync();
        }

        public async Task<ReportDto> GetFullReportAsync()
        {
            return new ReportDto
            {
                Top10Queries = (await GetTop10QueriesAsync()).ToList(),
                Top10Results = (await GetTop10ResultsAsync()).ToList(),
                ComicsOver100Results = (await GetComicsInMoreThan100ResultsAsync()).ToList(),
                TotalSearches = await _context.SearchLogs.CountAsync(),
                TotalUniqueComicsReturned = await _context.ResultLogs.CountAsync(),
                ReportGeneratedAt = DateTime.UtcNow
            };
        }

        public async Task ClearReportDataAsync()
        {
            _context.SearchLogs.RemoveRange(_context.SearchLogs);
            _context.ResultLogs.RemoveRange(_context.ResultLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalSearchCountAsync()
        {
            return await _context.SearchLogs.CountAsync();
        }

        private string GenerateQueryHash(SearchQuery query)
        {
            var hashInput = $"{query.QueryText}|{query.Genre}|{query.Author}|{query.Year}|{query.Edition}|{query.Language}|{query.NameType}";
            return hashInput.GetHashCode().ToString();
        }
    }
}
