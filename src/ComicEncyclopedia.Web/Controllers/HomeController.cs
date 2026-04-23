using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ComicEncyclopedia.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IComicService _comicService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IComicService comicService, ILogger<HomeController> logger)
        {
            _comicService = comicService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                IsDataLoaded = _comicService.IsDataLoaded,
                TotalComics = _comicService.TotalComicsCount,
                FilteredComics = _comicService.FilteredComicsCount,
                LastLoadedDate = _comicService.LastLoadedDate,
                WelcomeMessage = "Welcome to Fantasy B'zaar Comic Encyclopedia"
            };

            if (_comicService.IsDataLoaded)
            {
                var allComics = _comicService.GetAllComics().Take(100).ToList();
                if (allComics.Any())
                {
                    var random = new Random();
                    viewModel.FeaturedComics = allComics
                        .OrderBy(x => random.Next())
                        .Take(6)
                        .Select(c => _comicService.MapToDto(c))
                        .ToList();
                }
            }

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
