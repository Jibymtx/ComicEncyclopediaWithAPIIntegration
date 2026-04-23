using ComicEncyclopedia.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComicEncyclopedia.Business.Services
{
    public class BookCoverService : IBookCoverService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookCoverService> _logger;
        private readonly string? _googleBooksApiKey;

        public BookCoverService(HttpClient httpClient, ILogger<BookCoverService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _googleBooksApiKey = configuration["GoogleBooks:ApiKey"];
        }

        // fetch cover image
        public async Task<string?> GetCoverUrlAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn) || isbn == "missing")
            {
                return null;
            }

            var cleanIsbn = CleanIsbn(isbn);

            var openLibraryCover = await GetOpenLibraryCoverAsync(cleanIsbn);
            if (!string.IsNullOrEmpty(openLibraryCover))
            {
                return openLibraryCover;
            }

            var googleBooksCover = await GetGoogleBooksCoverAsync(cleanIsbn);
            if (!string.IsNullOrEmpty(googleBooksCover))
            {
                return googleBooksCover;
            }

            return null;
        }

        public async Task<BookInfo?> GetBookInfoAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn) || isbn == "missing")
            {
                return null;
            }

            var cleanIsbn = CleanIsbn(isbn);

            var openLibraryInfo = await GetOpenLibraryBookInfoAsync(cleanIsbn);
            if (openLibraryInfo != null)
            {
                return openLibraryInfo;
            }

            var googleBooksInfo = await GetGoogleBooksInfoAsync(cleanIsbn);
            if (googleBooksInfo != null)
            {
                return googleBooksInfo;
            }

            return null;
        }


        private string CleanIsbn(string isbn)
        {
            return new string(isbn.Where(c => char.IsDigit(c) || c == 'X' || c == 'x').ToArray());
        }

        private async Task<string?> GetOpenLibraryCoverAsync(string isbn)
        {
            try
            {
                var coverUrl = $"https://covers.openlibrary.org/b/isbn/{isbn}-M.jpg";

                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, coverUrl));

                if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 1000)
                {
                    return coverUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Open Library cover for ISBN {ISBN}", isbn);
            }

            return null;
        }

        private async Task<BookInfo?> GetOpenLibraryBookInfoAsync(string isbn)
        {
            try
            {
                var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
                var response = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                if (root.TryGetProperty($"ISBN:{isbn}", out var bookData))
                {
                    var info = new BookInfo
                    {
                        Source = "Open Library"
                    };

                    if (bookData.TryGetProperty("title", out var title))
                        info.Title = title.GetString();

                    if (bookData.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0)
                        info.Author = authors[0].GetProperty("name").GetString();

                    if (bookData.TryGetProperty("publishers", out var publishers) && publishers.GetArrayLength() > 0)
                        info.Publisher = publishers[0].GetProperty("name").GetString();

                    if (bookData.TryGetProperty("publish_date", out var pubDate))
                        info.PublishedDate = pubDate.GetString();

                    if (bookData.TryGetProperty("cover", out var cover))
                    {
                        if (cover.TryGetProperty("medium", out var mediumCover))
                            info.CoverUrl = mediumCover.GetString();
                    }

                    return info;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Open Library info for ISBN {ISBN}", isbn);
            }

            return null;
        }
        private async Task<string?> GetGoogleBooksCoverAsync(string isbn)
        {
            try
            {
                var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}";
                if (!string.IsNullOrEmpty(_googleBooksApiKey))
                {
                    url += $"&key={_googleBooksApiKey}";
                }

                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var volumeInfo = items[0].GetProperty("volumeInfo");
                    if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
                    {
                        if (imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                        {
                            return thumbnail.GetString()?.Replace("http://", "https://");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Google Books cover for ISBN {ISBN}", isbn);
            }

            return null;
        }

        private async Task<BookInfo?> GetGoogleBooksInfoAsync(string isbn)
        {
            try
            {
                var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}";
                if (!string.IsNullOrEmpty(_googleBooksApiKey))
                {
                    url += $"&key={_googleBooksApiKey}";
                }

                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                {
                    var volumeInfo = items[0].GetProperty("volumeInfo");

                    var info = new BookInfo
                    {
                        Source = "Google Books"
                    };

                    if (volumeInfo.TryGetProperty("title", out var title))
                        info.Title = title.GetString();

                    if (volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0)
                        info.Author = authors[0].GetString();

                    if (volumeInfo.TryGetProperty("publisher", out var publisher))
                        info.Publisher = publisher.GetString();

                    if (volumeInfo.TryGetProperty("publishedDate", out var pubDate))
                        info.PublishedDate = pubDate.GetString();

                    if (volumeInfo.TryGetProperty("description", out var description))
                        info.Description = description.GetString();

                    if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
                    {
                        if (imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                            info.CoverUrl = thumbnail.GetString()?.Replace("http://", "https://");
                    }

                    return info;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching Google Books info for ISBN {ISBN}", isbn);
            }

            return null;
        }
    }
}
