namespace ComicEncyclopedia.Common.DTOs
{
    public class SearchResultDto
    {
        public string ComicTitle { get; set; } = string.Empty;
        public string ComicBLRecordID { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int TimesReturned { get; set; }
        public DateTime LastReturnedDate { get; set; }
    }

    public class SearchQueryDto
    {
        public string QueryDescription { get; set; } = string.Empty;
        public int TimesSearched { get; set; }
        public int TotalResultsReturned { get; set; }
        public DateTime LastSearchedDate { get; set; }
    }
}
