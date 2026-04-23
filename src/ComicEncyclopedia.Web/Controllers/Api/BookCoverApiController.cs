using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComicEncyclopedia.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookCoverApiController : ControllerBase
    {
        private readonly IBookCoverService _bookCoverService;
        private readonly IComicService _comicService;
        private readonly ILogger<BookCoverApiController> _logger;

        public BookCoverApiController(
            IBookCoverService bookCoverService,
            IComicService comicService,
            ILogger<BookCoverApiController> logger)
        {
            _bookCoverService = bookCoverService;
            _comicService = comicService;
            _logger = logger;
        }

        // fetch cover image
        [HttpGet("isbn/{isbn}")]
        public async Task<ActionResult> GetCoverByIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { error = "ISBN is required" });
            }

            var coverUrl = await _bookCoverService.GetCoverUrlAsync(isbn);

            if (string.IsNullOrEmpty(coverUrl))
            {
                return NotFound(new { error = "Cover not found", isbn });
            }

            return Ok(new { isbn, coverUrl });
        }


        [HttpGet("info/{isbn}")]
        public async Task<ActionResult> GetBookInfoByIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
            {
                return BadRequest(new { error = "ISBN is required" });
            }

            var bookInfo = await _bookCoverService.GetBookInfoAsync(isbn);

            if (bookInfo == null)
            {
                return NotFound(new { error = "Book info not found", isbn });
            }

            return Ok(bookInfo);
        }

        [HttpGet("comic/{comicId}")]
        public async Task<ActionResult> GetCoverByComicId(string comicId)
        {
            var comic = _comicService.GetComicById(comicId);

            if (comic == null)
            {
                return NotFound(new { error = "Comic not found", comicId });
            }

            if (string.IsNullOrWhiteSpace(comic.ISBN) || comic.ISBN == "missing")
            {
                return NotFound(new { error = "Comic has no ISBN", comicId });
            }

            var coverUrl = await _bookCoverService.GetCoverUrlAsync(comic.ISBN);

            if (string.IsNullOrEmpty(coverUrl))
            {
                return NotFound(new { error = "Cover not found", comicId, isbn = comic.ISBN });
            }

            return Ok(new
            {
                comicId,
                isbn = comic.ISBN,
                title = comic.Title,
                coverUrl
            });
        }

        [HttpGet("comic/{comicId}/info")]
        public async Task<ActionResult> GetBookInfoByComicId(string comicId)
        {
            var comic = _comicService.GetComicById(comicId);

            if (comic == null)
            {
                return NotFound(new { error = "Comic not found", comicId });
            }

            if (string.IsNullOrWhiteSpace(comic.ISBN) || comic.ISBN == "missing")
            {
                return NotFound(new { error = "Comic has no ISBN", comicId });
            }

            var bookInfo = await _bookCoverService.GetBookInfoAsync(comic.ISBN);

            if (bookInfo == null)
            {
                return NotFound(new { error = "Book info not found", comicId, isbn = comic.ISBN });
            }

            return Ok(new
            {
                comicId,
                localTitle = comic.Title,
                localAuthor = comic.Author,
                externalInfo = bookInfo
            });
        }
    }
}
