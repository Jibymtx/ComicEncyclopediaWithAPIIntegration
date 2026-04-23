using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using ComicEncyclopedia.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ComicEncyclopedia.Tests
{
    public class BlackBoxTests
    {
        private static ComicsApiController BuildComicsApi(
            out Mock<IComicService> comicService,
            out Mock<IReportService> reportService)
        {
            var comics = TestData.SampleComics();
            comicService = new Mock<IComicService>();
            reportService = new Mock<IReportService>();

            comicService.Setup(s => s.GetAllComics()).Returns(comics);
            comicService.Setup(s => s.TotalComicsCount).Returns(comics.Count);
            comicService.Setup(s => s.FilteredComicsCount).Returns(comics.Count);
            comicService.Setup(s => s.IsDataLoaded).Returns(true);
            comicService.Setup(s => s.LastLoadedDate).Returns(new System.DateTime(2026, 1, 1));
            comicService.Setup(s => s.MapToDto(It.IsAny<Comic>()))
                .Returns<Comic>(c => new ComicDto
                {
                    BLRecordID = c.BLRecordID,
                    Title = c.Title,
                    Author = c.Author,
                    Genre = c.Genre,
                    Publisher = c.Publisher,
                    DateOfPublication = c.DateOfPublication,
                    DisplayISBN = c.DisplayISBN
                });
            comicService.Setup(s => s.MapToDtos(It.IsAny<IEnumerable<Comic>>()))
                .Returns<IEnumerable<Comic>>(list => list.Select(c => new ComicDto
                {
                    BLRecordID = c.BLRecordID,
                    Title = c.Title,
                    Author = c.Author,
                    Genre = c.Genre,
                    Publisher = c.Publisher,
                    DateOfPublication = c.DateOfPublication,
                    DisplayISBN = c.DisplayISBN
                }));
            comicService.Setup(s => s.GetComicDtoById("001"))
                .Returns(new ComicDto { BLRecordID = "001", Title = "Batman: Year One", Author = "Frank Miller" });
            comicService.Setup(s => s.GetComicDtoById(It.Is<string>(id => id != "001")))
                .Returns((ComicDto?)null);
            comicService.Setup(s => s.SearchComics(It.IsAny<SearchQuery>()))
                .Returns<SearchQuery>(q =>
                {
                    if (string.IsNullOrWhiteSpace(q.QueryText)) return comics;
                    return comics.Where(c => c.Title.Contains(q.QueryText, System.StringComparison.OrdinalIgnoreCase));
                });
            comicService.Setup(s => s.GetAvailableGenres())
                .Returns(new[] { "Superhero", "Biography" });

            return new ComicsApiController(
                comicService.Object,
                reportService.Object,
                NullLogger<ComicsApiController>.Instance);
        }

        private static object? GetProp(object obj, string name)
        {
            return obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!.GetValue(obj);
        }

        [Fact(DisplayName = "BB-01: ComicsApi returns list")]
        public void BB01_ComicsApi_GetAll_Returns_List()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            Assert.NotNull(ok.Value);

            var data = GetProp(ok.Value!, "data") as IEnumerable<ComicDto>;
            Assert.NotNull(data);
            Assert.Equal(3, data!.Count());

            var totalCount = (int)GetProp(ok.Value!, "totalCount")!;
            Assert.Equal(3, totalCount);
        }

        [Fact(DisplayName = "BB-02: ComicsApi valid ID returns comic")]
        public void BB02_ComicsApi_GetById_Valid_Returns_Comic()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.GetById("001");

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var dto = Assert.IsType<ComicDto>(ok.Value);
            Assert.Equal("001", dto.BLRecordID);
            Assert.Equal("Batman: Year One", dto.Title);
        }

        [Fact(DisplayName = "BB-03: ComicsApi invalid ID returns 404")]
        public void BB03_ComicsApi_GetById_Invalid_Returns_404()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.GetById("does-not-exist");

            Assert.IsType<NotFoundObjectResult>(action.Result);
        }

        [Fact(DisplayName = "BB-04: Search with query returns filtered results")]
        public void BB04_ComicsApi_Search_Returns_Filtered_Results()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.Search(title: "Batman");

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var data = GetProp(ok.Value!, "data") as IEnumerable<ComicDto>;
            Assert.NotNull(data);

            var list = data!.ToList();
            Assert.Single(list);
            Assert.Contains("Batman", list[0].Title);
        }

        [Fact(DisplayName = "BB-05: Stats endpoint returns statistics")]
        public void BB05_ComicsApi_Stats_Returns_Statistics()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(action);
            Assert.NotNull(ok.Value);
            Assert.Equal(3, (int)GetProp(ok.Value!, "totalComics")!);
            Assert.Equal(3, (int)GetProp(ok.Value!, "filteredComics")!);
            Assert.True((bool)GetProp(ok.Value!, "isDataLoaded")!);
        }

        [Fact(DisplayName = "BB-06: Genres endpoint returns genre list")]
        public void BB06_ComicsApi_Genres_Returns_List()
        {
            var controller = BuildComicsApi(out _, out _);

            var action = controller.GetGenres();

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            var genres = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value);
            var list = genres.ToList();
            Assert.Contains("Superhero", list);
            Assert.Contains("Biography", list);
        }

        [Fact(DisplayName = "BB-07: BookCoverApi valid ISBN returns cover URL")]
        public async Task BB07_BookCoverApi_Valid_Isbn_Returns_Cover()
        {
            var coverService = new Mock<IBookCoverService>();
            coverService.Setup(s => s.GetCoverUrlAsync("9780930289331"))
                .ReturnsAsync("https://covers.openlibrary.org/b/isbn/9780930289331-M.jpg");

            var comicService = new Mock<IComicService>();

            var controller = new BookCoverApiController(
                coverService.Object,
                comicService.Object,
                NullLogger<BookCoverApiController>.Instance);

            var action = await controller.GetCoverByIsbn("9780930289331");

            var ok = Assert.IsType<OkObjectResult>(action);
            var coverUrl = GetProp(ok.Value!, "coverUrl") as string;
            Assert.False(string.IsNullOrWhiteSpace(coverUrl));
            Assert.StartsWith("https://", coverUrl);
        }

        [Fact(DisplayName = "BB-08: BookCoverApi invalid ISBN returns 404")]
        public async Task BB08_BookCoverApi_Invalid_Isbn_Returns_404()
        {
            var coverService = new Mock<IBookCoverService>();
            coverService.Setup(s => s.GetCoverUrlAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            var comicService = new Mock<IComicService>();

            var controller = new BookCoverApiController(
                coverService.Object,
                comicService.Object,
                NullLogger<BookCoverApiController>.Instance);

            var action = await controller.GetCoverByIsbn("0000000000");

            Assert.IsType<NotFoundObjectResult>(action);
        }
    }
}
