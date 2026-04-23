namespace ComicEncyclopedia.Data.Entities
{
    public class ResultLog
    {
        public int Id { get; set; }
        public string BLRecordID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int TimesReturned { get; set; } = 0;
        public DateTime FirstReturnedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastReturnedDate { get; set; } = DateTime.UtcNow;
    }
}
