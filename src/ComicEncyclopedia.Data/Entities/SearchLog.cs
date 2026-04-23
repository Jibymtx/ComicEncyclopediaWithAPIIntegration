namespace ComicEncyclopedia.Data.Entities
{
    public class SearchLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string QueryText { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Author { get; set; }
        public int? Year { get; set; }
        public string? Edition { get; set; }
        public string? Language { get; set; }
        public string? NameType { get; set; }
        public int ResultCount { get; set; }
        public DateTime SearchTime { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public string QueryHash { get; set; } = string.Empty;

        public virtual ApplicationUser? User { get; set; }
    }
}
