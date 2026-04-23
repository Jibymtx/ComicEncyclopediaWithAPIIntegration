using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ComicEncyclopedia.Business.Helpers;
using ComicEncyclopedia.Business.Services;
using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Web.Controllers.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using Xunit;

namespace ComicEncyclopedia.Tests
{
    public class WhiteBoxTests
    {
        [Fact(DisplayName = "WB-01: API returns valid JSON")]
        public void WB01_Api_Returns_Valid_Json()
        {
            var comicService = new Mock<IComicService>();
            comicService.Setup(s => s.TotalComicsCount).Returns(3);
            comicService.Setup(s => s.FilteredComicsCount).Returns(3);
            comicService.Setup(s => s.IsDataLoaded).Returns(true);
            comicService.Setup(s => s.LastLoadedDate).Returns(new System.DateTime(2026, 1, 1));

            var reportService = new Mock<IReportService>();
            var controller = new ComicsApiController(
                comicService.Object,
                reportService.Object,
                NullLogger<ComicsApiController>.Instance);

            var result = controller.GetStats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(ok.Value);

            Assert.False(string.IsNullOrWhiteSpace(json));
            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
            Assert.Equal(3, doc.RootElement.GetProperty("totalComics").GetInt32());
        }

        [Fact(DisplayName = "WB-02: SQL injection prevented (test TextSanitizer)")]
        public void WB02_TextSanitizer_Strips_SqlInjection_Payloads()
        {
            var sanitizer = new TextSanitizer();
            var payload = "admin'; DROP TABLE Users;--\0\x01\x1F";

            var result = sanitizer.SanitizeText(payload);

            Assert.False(result.Contains('\0'));
            Assert.False(result.Contains('\x01'));
            Assert.False(result.Contains('\x1F'));
            Assert.Equal("admin'; DROP TABLE Users;--", result);
        }

        [Fact(DisplayName = "WB-03: Null input handling")]
        public void WB03_TextSanitizer_Handles_Null_Input()
        {
            var sanitizer = new TextSanitizer();

            var fromNull = sanitizer.SanitizeText(null!);
            var fromEmpty = sanitizer.SanitizeText(string.Empty);

            Assert.Equal(string.Empty, fromNull);
            Assert.Equal(string.Empty, fromEmpty);
            Assert.Empty(sanitizer.ParseMultipleValues(null!));
        }

        [Fact(DisplayName = "WB-04: XSS attack prevented (TextSanitizer removes script tags)")]
        public void WB04_TextSanitizer_Removes_Script_Tags()
        {
            var sanitizer = new TextSanitizer();
            var payload = "<p>Hi</p><script>alert('xss')</script><b>Bye</b>";

            var result = sanitizer.StripHtml(payload);

            Assert.DoesNotContain("<script", result, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("</script>", result, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<p>", result);
            Assert.DoesNotContain("<b>", result);
            Assert.Contains("Hi", result);
            Assert.Contains("Bye", result);
        }

        [Fact(DisplayName = "WB-05: BookCoverService fallback logic")]
        public async Task WB05_BookCoverService_FallsBack_From_OpenLibrary_To_GoogleBooks()
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Head &&
                        r.RequestUri!.Host.Contains("openlibrary")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var googleJson = "{\"items\":[{\"volumeInfo\":{\"imageLinks\":{\"thumbnail\":\"http://books.google.com/books?id=xyz&img=1\"}}}]}";
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get &&
                        r.RequestUri!.Host.Contains("googleapis")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(googleJson)
                });

            var httpClient = new HttpClient(handler.Object);
            var config = new ConfigurationBuilder().Build();
            var service = new BookCoverService(httpClient, NullLogger<BookCoverService>.Instance, config);

            var coverUrl = await service.GetCoverUrlAsync("9780930289331");

            Assert.NotNull(coverUrl);
            Assert.Contains("books.google.com", coverUrl);
            Assert.StartsWith("https://", coverUrl);
        }

        [Fact(DisplayName = "WB-06: EmailService fallback logic")]
        public async Task WB06_EmailService_FallsBack_To_SendGrid_For_Unknown_Provider()
        {
            var sendGrid = new Mock<ISendGridEmailService>();
            var mailjet = new Mock<IMailjetEmailService>();

            sendGrid.Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
                .ReturnsAsync(new EmailResult { Success = true, Provider = "SendGrid", StatusCode = 200 });
            mailjet.Setup(s => s.SendAsync(It.IsAny<EmailRequest>()))
                .ReturnsAsync(new EmailResult { Success = true, Provider = "Mailjet", StatusCode = 200 });

            var service = new EmailService(
                sendGrid.Object,
                mailjet.Object,
                NullLogger<EmailService>.Instance);

            var result = await service.SendAsync(
                new EmailRequest { ToEmail = "a@b.com", Subject = "x", BodyText = "y" },
                (EmailProvider)99);

            Assert.True(result.Success);
            Assert.Equal("SendGrid", result.Provider);
            sendGrid.Verify(s => s.SendAsync(It.IsAny<EmailRequest>()), Times.Once);
            mailjet.Verify(s => s.SendAsync(It.IsAny<EmailRequest>()), Times.Never);
        }

        [Fact(DisplayName = "WB-07: Input sanitization works")]
        public void WB07_TextSanitizer_Cleans_UnicodeAndControlChars()
        {
            var sanitizer = new TextSanitizer();
            var input = "Café “Batman”—issue\t1\x02";

            var result = sanitizer.SanitizeText(input);

            Assert.Contains("Cafe", result);
            Assert.Contains("\"Batman\"", result);
            Assert.Contains("-", result);
            Assert.False(result.Contains('\x02'));
            Assert.False(result.Contains('é'));
            Assert.False(result.Contains('—'));
        }

        [Fact(DisplayName = "WB-08: Email validation rejects invalid format")]
        public void WB08_EmailValidation_Rejects_Invalid_Format()
        {
            Assert.False(TextSanitizer.IsValidEmail(""));
            Assert.False(TextSanitizer.IsValidEmail("   "));
            Assert.False(TextSanitizer.IsValidEmail(null));
            Assert.False(TextSanitizer.IsValidEmail("not-an-email"));
            Assert.False(TextSanitizer.IsValidEmail("@nouser.com"));
            Assert.False(TextSanitizer.IsValidEmail("two@@at.com"));

            Assert.True(TextSanitizer.IsValidEmail("user@example.com"));
            Assert.True(TextSanitizer.IsValidEmail("first.last+tag@sub.example.co.uk"));
        }
    }
}
