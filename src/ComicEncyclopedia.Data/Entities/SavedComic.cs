namespace ComicEncyclopedia.Data.Entities
{
    public class SavedComic
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string BLRecordID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string? DateOfPublication { get; set; }
        public string? ISBN { get; set; }
        public DateTime SavedDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public string? SerializedComicData { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
