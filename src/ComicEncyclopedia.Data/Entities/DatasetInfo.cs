namespace ComicEncyclopedia.Data.Entities
{
    public class DatasetInfo
    {
        public int Id { get; set; }
        public string SourceUrl { get; set; } = string.Empty;
        public string? FileHash { get; set; }
        public DateTime LastDownloadDate { get; set; }
        public DateTime LastCheckDate { get; set; }
        public int RecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }
}
