using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComicEncyclopedia.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComicsApiController : ControllerBase
    {
        private readonly IComicService _comicService;
        private readonly IReportService _reportService;
        private readonly ILogger<ComicsApiController> _logger;

        public ComicsApiController(
            IComicService comicService,
            IReportService reportService,
            ILogger<ComicsApiController> logger)
        {
            _comicService = comicService;
            _reportService = reportService;
            _logger = logger;
        }

        // get all comics
        [HttpGet]
        public ActionResult<IEnumerable<ComicDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var comics = _comicService.GetAllComics()
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var dtos = _comicService.MapToDtos(comics);

            return Ok(new
            {
                page,
                pageSize,
                totalCount = _comicService.FilteredComicsCount,
                data = dtos
            });
        }


        [HttpGet("{id}")]
        public ActionResult<ComicDto> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { error = "Comic ID is required" });
            }

            var comic = _comicService.GetComicDtoById(id);

            if (comic == null)
            {
                return NotFound(new { error = "Comic not found", id });
            }

            return Ok(comic);
        }

        [HttpGet("search")]
        public ActionResult<IEnumerable<ComicDto>> Search(
            [FromQuery] string? title = null,
            [FromQuery] string? author = null,
            [FromQuery] string? genre = null,
            [FromQuery] int? year = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = new SearchQuery
            {
                QueryText = title ?? "",
                Author = author,
                Genre = genre,
                Year = year
            };

            var results = _comicService.SearchComics(query);
            var totalCount = results.Count();

            var pagedResults = results
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var dtos = _comicService.MapToDtos(pagedResults);

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                query = new { title, author, genre, year },
                data = dtos
            });
        }

        [HttpGet("genres")]
        public ActionResult<IEnumerable<string>> GetGenres()
        {
            var genres = _comicService.GetAvailableGenres();
            return Ok(genres);
        }

        [HttpGet("authors")]
        public ActionResult<IEnumerable<string>> GetAuthors([FromQuery] int limit = 100)
        {
            var authors = _comicService.GetAvailableAuthors().Take(limit);
            return Ok(authors);
        }

        [HttpGet("years")]
        public ActionResult<IEnumerable<int>> GetYears()
        {
            var years = _comicService.GetAvailableYears();
            return Ok(years);
        }

        [HttpGet("stats")]
        public ActionResult GetStats()
        {
            return Ok(new
            {
                totalComics = _comicService.TotalComicsCount,
                filteredComics = _comicService.FilteredComicsCount,
                isDataLoaded = _comicService.IsDataLoaded,
                lastLoadedDate = _comicService.LastLoadedDate
            });
        }

        [HttpGet("analytics")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult> GetAnalytics()
        {
            var report = await _reportService.GetFullReportAsync();

            return Ok(new
            {
                totalSearches = report.TotalSearches,
                top10Queries = report.Top10Queries,
                top10Results = report.Top10Results
            });
        }
    }
}
