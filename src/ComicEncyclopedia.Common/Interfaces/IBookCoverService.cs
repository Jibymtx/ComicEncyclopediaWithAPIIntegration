namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IBookCoverService
    {
        // fetch cover image
        Task<string?> GetCoverUrlAsync(string isbn);
        Task<BookInfo?> GetBookInfoAsync(string isbn);
    }

    public class BookInfo
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public string? Source { get; set; }
    }
}
