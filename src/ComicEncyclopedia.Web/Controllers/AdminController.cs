using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComicEncyclopedia.Web.Controllers
{
    [Authorize(Roles = "Staff,Admin")]
    public class AdminController : Controller
    {
        private readonly IComicService _comicService;
        private readonly IReportService _reportService;
        private readonly IFlagService _flagService;
        private readonly IDatasetUpdateService _datasetUpdateService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IComicService comicService,
            IReportService reportService,
            IFlagService flagService,
            IDatasetUpdateService datasetUpdateService,
            ILogger<AdminController> logger)
        {
            _comicService = comicService;
            _reportService = reportService;
            _flagService = flagService;
            _datasetUpdateService = datasetUpdateService;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var report = await _reportService.GetFullReportAsync();
            var flaggedCount = await _flagService.GetFlaggedCountAsync();

            var viewModel = new ReportsViewModel
            {
                Report = report,
                TotalSearches = report.TotalSearches,
                FlaggedRecordsCount = flaggedCount,
                LastDatasetUpdate = _datasetUpdateService.LastUpdateDate,
                IsDatasetUpdateAvailable = await _datasetUpdateService.CheckForUpdatesAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TopQueries()
        {
            var queries = await _reportService.GetTop10QueriesAsync();
            return View(queries);
        }
        public async Task<IActionResult> TopResults()
        {
            var results = await _reportService.GetTop10ResultsAsync();
            return View(results);
        }

        public async Task<IActionResult> FrequentResults()
        {
            var results = await _reportService.GetComicsInMoreThan100ResultsAsync();
            return View(results);
        }

        public async Task<IActionResult> FlaggedRecords()
        {
            var flaggedComics = await _flagService.GetFlaggedRecordsAsync();
            var viewModel = flaggedComics.Select(c => _comicService.MapToDto(c)).ToList();
            return View(viewModel);
        }


        [HttpGet]
        public IActionResult FlagRecord(string id)
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

            var viewModel = new FlagRecordViewModel
            {
                BLRecordID = id,
                ComicTitle = comic.Title
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagRecord(FlagRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _flagService.FlagRecordAsync(model.BLRecordID, userId, model.Reason);
                _logger.LogInformation("Record {RecordId} flagged by user {UserId}", model.BLRecordID, userId);
                TempData["Success"] = "Record has been flagged for review.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging record {RecordId}", model.BLRecordID);
                TempData["Error"] = "Error flagging record.";
            }

            return RedirectToAction("Details", "Comics", new { id = model.BLRecordID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnflagRecord(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            try
            {
                await _flagService.UnflagRecordAsync(id);
                _logger.LogInformation("Record {RecordId} unflagged", id);
                TempData["Success"] = "Flag has been removed from the record.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unflagging record {RecordId}", id);
                TempData["Error"] = "Error removing flag from record.";
            }

            return RedirectToAction(nameof(FlaggedRecords));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckForUpdates()
        {
            var updateAvailable = await _datasetUpdateService.CheckForUpdatesAsync();

            if (updateAvailable)
            {
                TempData["Info"] = "A dataset update is available.";
            }
            else
            {
                TempData["Info"] = "Your dataset is up to date.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDataset()
        {
            if (_datasetUpdateService.IsUpdating)
            {
                TempData["Warning"] = "An update is already in progress.";
                return RedirectToAction(nameof(Dashboard));
            }

            var success = await _datasetUpdateService.UpdateDatasetAsync();

            if (success)
            {
                TempData["Success"] = "Dataset has been updated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to update dataset. Check logs for details.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearAnalytics()
        {
            await _reportService.ClearReportDataAsync();
            _logger.LogInformation("Analytics data cleared by admin");
            TempData["Success"] = "Analytics data has been cleared.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
