using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Enums;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using ComicEncyclopedia.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComicEncyclopedia.Web.Controllers
{
    public class ComicsController : Controller
    {
        private readonly IComicService _comicService;
        private readonly ISearchListService _searchListService;
        private readonly IReportService _reportService;
        private readonly IBookCoverService _bookCoverService;
        private readonly ILogger<ComicsController> _logger;
        private const int PageSize = 50;

        public ComicsController(
            IComicService comicService,
            ISearchListService searchListService,
            IReportService reportService,
            IBookCoverService bookCoverService,
            ILogger<ComicsController> logger)
        {
            _comicService = comicService;
            _searchListService = searchListService;
            _reportService = reportService;
            _bookCoverService = bookCoverService;
            _logger = logger;
        }

        // get all comics
        public async Task<IActionResult> Index(
            string? searchTitle,
            string? genre,
            string? author,
            int? year,
            SortOrder sortOrder = SortOrder.TitleAscending,
            GroupBy groupBy = GroupBy.None,
            int page = 1)
        {
            if (!_comicService.IsDataLoaded)
            {
                return View(new ComicListViewModel
                {
                    IsDataLoaded = false,
                    StatusMessage = "No data loaded. Please upload a CSV file to begin."
                });
            }

            var query = new SearchQuery
            {
                QueryText = searchTitle,
                Genre = genre,
                Author = author,
                Year = year
            };

            var results = _comicService.SearchComics(query);

            results = _comicService.SortComics(results, sortOrder);

            var totalCount = results.Count();

            if (!string.IsNullOrWhiteSpace(searchTitle) || !string.IsNullOrWhiteSpace(genre) ||
                !string.IsNullOrWhiteSpace(author) || year.HasValue)
            {
                query.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query.ResultCount = totalCount;
                await _reportService.RecordSearchAsync(query, results.Take(100));
            }

            var viewModel = new ComicListViewModel
            {
                IsDataLoaded = true,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = PageSize,
                SearchTitle = searchTitle,
                SelectedGenre = genre,
                SelectedAuthor = author,
                SelectedYear = year,
                SortOrder = sortOrder,
                GroupBy = groupBy,
                LastLoadedDate = _comicService.LastLoadedDate,
                AvailableGenres = _comicService.GetAvailableGenres().ToList(),
                AvailableAuthors = _comicService.GetAvailableAuthors().Take(500).ToList(),
                AvailableYears = _comicService.GetAvailableYears().ToList(),
                AvailableLanguages = _comicService.GetAvailableLanguages().ToList()
            };

            if (groupBy == GroupBy.Author)
            {
                var grouped = _comicService.GroupComicsByAuthor(results);
                viewModel.GroupedByAuthor = grouped.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(c => _comicService.MapToDto(c)).ToList()
                );
                viewModel.DisplayedCount = totalCount;
            }
            else if (groupBy == GroupBy.Year)
            {
                var grouped = _comicService.GroupComicsByYear(results);
                viewModel.GroupedByYear = grouped.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(c => _comicService.MapToDto(c)).ToList()
                );
                viewModel.DisplayedCount = totalCount;
            }
            else
            {
                var pagedResults = results
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize);

                viewModel.Comics = pagedResults.Select(c => _comicService.MapToDto(c)).ToList();
                viewModel.DisplayedCount = viewModel.Comics.Count;
            }

            viewModel.StatusMessage = $"Showing {viewModel.DisplayedCount} of {totalCount} comics";

            return View(viewModel);
        }


        public async Task<IActionResult> Details(string id)
        {
            // check if null
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var comic = _comicService.GetComicById(id);
            if (comic == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isInList = false;

            if (!string.IsNullOrWhiteSpace(userId))
            {
                isInList = await _searchListService.ContainsAsync(userId, id);
            }

            var viewModel = new ComicDetailsViewModel
            {
                Comic = _comicService.MapToDto(comic),
                IsInSearchList = isInList,
                IsLoggedIn = User.Identity?.IsAuthenticated ?? false,
                CanFlag = User.IsInRole("Staff") || User.IsInRole("Admin")
            };

            if (!string.IsNullOrWhiteSpace(comic.AllNames))
            {
                viewModel.FormattedAllNames = comic.AllNames
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(comic.Topics))
            {
                viewModel.FormattedTopics = comic.Topics
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }

            try
            {
                // fetch cover image
                if (!string.IsNullOrWhiteSpace(comic.ISBN) && comic.ISBN != "missing")
                {
                    viewModel.CoverUrl = await _bookCoverService.GetCoverUrlAsync(comic.ISBN);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch book cover for ISBN {ISBN}", comic.ISBN);
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AdvancedSearch()
        {
            var viewModel = new AdvancedSearchViewModel
            {
                AvailableGenres = _comicService.GetAvailableGenres().ToList(),
                AvailableYears = _comicService.GetAvailableYears().ToList(),
                AvailableLanguages = _comicService.GetAvailableLanguages().ToList(),
                AvailableNameTypes = _comicService.GetAvailableNameTypes().ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AdvancedSearch(AdvancedSearchViewModel model)
        {
            return RedirectToAction(nameof(Index), new
            {
                searchTitle = model.Title,
                genre = model.Genre,
                author = model.Author,
                year = model.Year,
                sortOrder = SortOrder.TitleAscending
            });
        }


        [HttpGet]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(200_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid CSV file.";
                return View();
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Only CSV files are allowed.";
                return View();
            }

            string? tempFilePath = null;

            try
            {
                _logger.LogInformation("Starting CSV upload. File size: {Size} bytes", file.Length);

                tempFilePath = Path.Combine(Path.GetTempPath(), $"comic_upload_{Guid.NewGuid()}.csv");

                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
                {
                    await file.CopyToAsync(fileStream);
                }

                _logger.LogInformation("File saved to temp path: {Path}", tempFilePath);

                var repository = HttpContext.RequestServices.GetRequiredService<IComicRepository>();

                if (repository is ComicEncyclopedia.Data.Repositories.ComicRepository concreteRepo)
                {
                    concreteRepo.SetFilePath(tempFilePath);
                    await repository.LoadAllComicsAsync();
                }

                var count = repository.FilteredCount;

                TempData["Success"] = $"Successfully loaded {count:N0} comics (Fantasy/Horror/Sci-Fi) from {repository.Count:N0} total records.";
                _logger.LogInformation("CSV uploaded successfully. Total: {Total}, Filtered: {Filtered}", repository.Count, count);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // return error
                _logger.LogError(ex, "Error uploading CSV file: {Message}", ex.Message);
                TempData["Error"] = $"Error loading file: {ex.Message}";
                return View();
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file: {Path}", tempFilePath);
                    }
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Staff,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult ClearData()
        {
            _comicService.ClearData();
            TempData["Success"] = "All comic data has been cleared.";
            return RedirectToAction(nameof(Index));
        }
    }
}
