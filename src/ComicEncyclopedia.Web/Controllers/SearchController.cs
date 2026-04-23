using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComicEncyclopedia.Web.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IComicService _comicService;
        private readonly ISearchListService _searchListService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IComicService comicService,
            ISearchListService searchListService,
            ILogger<SearchController> logger)
        {
            _comicService = comicService;
            _searchListService = searchListService;
            _logger = logger;
        }

        public async Task<IActionResult> MyList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var savedComics = await _searchListService.GetSearchListAsync(userId);

            var viewModel = new SearchListViewModel
            {
                SavedComics = savedComics.Select(c => _comicService.MapToDto(c)).ToList(),
                Count = savedComics.Count()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string comicId, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(comicId))
            {
                TempData["Error"] = "Invalid comic ID.";
                return RedirectToLocal(returnUrl);
            }

            var comic = _comicService.GetComicById(comicId);
            if (comic == null)
            {
                // return error
                TempData["Error"] = "Comic not found.";
                return RedirectToLocal(returnUrl);
            }

            try
            {
                await _searchListService.AddToSearchListAsync(userId, comic);
                TempData["Success"] = $"'{comic.Title}' added to your search list.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comic to search list");
                TempData["Error"] = "Error adding comic to search list.";
            }

            return RedirectToLocal(returnUrl);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string comicId, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(comicId))
            {
                TempData["Error"] = "Invalid comic ID.";
                return RedirectToLocal(returnUrl);
            }

            try
            {
                await _searchListService.RemoveFromSearchListAsync(userId, comicId);
                TempData["Success"] = "Comic removed from your search list.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing comic from search list");
                TempData["Error"] = "Error removing comic from search list.";
            }

            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            try
            {
                await _searchListService.ClearSearchListAsync(userId);
                TempData["Success"] = "Your search list has been cleared.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing search list");
                TempData["Error"] = "Error clearing search list.";
            }

            return RedirectToAction(nameof(MyList));
        }

        [HttpGet]
        public async Task<IActionResult> IsInList(string comicId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Json(new { isInList = false });
            }

            var isInList = await _searchListService.ContainsAsync(userId, comicId);
            return Json(new { isInList });
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(MyList));
        }
    }
}
