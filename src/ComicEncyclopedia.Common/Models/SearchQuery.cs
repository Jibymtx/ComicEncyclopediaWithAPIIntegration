namespace ComicEncyclopedia.Common.Models
{
    public class SearchQuery
    {
        public int Id { get; set; }
        public string QueryText { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? Author { get; set; }
        public int? Year { get; set; }
        public string? Edition { get; set; }
        public string? Language { get; set; }
        public string? NameType { get; set; }
        public DateTime SearchTime { get; set; } = DateTime.Now;
        public int ResultCount { get; set; }
        public string? UserId { get; set; }

        public override string ToString()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(QueryText)) parts.Add($"Text: {QueryText}");
            if (!string.IsNullOrWhiteSpace(Genre)) parts.Add($"Genre: {Genre}");
            if (!string.IsNullOrWhiteSpace(Author)) parts.Add($"Author: {Author}");
            if (Year.HasValue) parts.Add($"Year: {Year}");
            if (!string.IsNullOrWhiteSpace(Edition)) parts.Add($"Edition: {Edition}");
            if (!string.IsNullOrWhiteSpace(Language)) parts.Add($"Language: {Language}");
            if (!string.IsNullOrWhiteSpace(NameType)) parts.Add($"Name Type: {NameType}");

            return parts.Count > 0 ? string.Join(", ", parts) : "All Comics";
        }


        public override bool Equals(object? obj) {
            if (obj is SearchQuery other)
            {
                return QueryText == other.QueryText &&
                       Genre == other.Genre &&
                       Author == other.Author &&
                       Year == other.Year &&
                       Edition == other.Edition &&
                       Language == other.Language &&
                       NameType == other.NameType;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(QueryText, Genre, Author, Year, Edition, Language, NameType);
        }
    }
}
