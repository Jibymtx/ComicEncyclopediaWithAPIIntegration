namespace ComicEncyclopedia.Common.DTOs
{
    public class ReportDto
    {
        public List<SearchQueryDto> Top10Queries { get; set; } = new();
        public List<SearchResultDto> Top10Results { get; set; } = new();
        public List<SearchResultDto> ComicsOver100Results { get; set; } = new();
        public int TotalSearches { get; set; }
        public int TotalUniqueComicsReturned { get; set; }
        public DateTime ReportGeneratedAt { get; set; } = DateTime.Now;
    }
}
