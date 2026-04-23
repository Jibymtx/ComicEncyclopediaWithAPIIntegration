namespace ComicEncyclopedia.Common.Models
{
    public class SearchResult
    {
        public int Id { get; set; }
        public Comic Comic { get; set; } = null!;
        public string ComicBLRecordID { get; set; } = string.Empty;
        public int TimesReturned { get; set; } = 1;
        public DateTime LastReturnedDate { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{Comic?.Title ?? "Unknown"} (Returned {TimesReturned} times)";
        }
    }
}
