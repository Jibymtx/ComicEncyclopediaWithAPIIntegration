namespace ComicEncyclopedia.Data.Entities
{
    public class FlaggedRecord
    {
        public int Id { get; set; }
        public string BLRecordID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FlaggedByUserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime FlaggedDate { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedDate { get; set; }
        public string? ResolvedByUserId { get; set; }
        public string? ResolutionNotes { get; set; }

        public virtual ApplicationUser? FlaggedByUser { get; set; }
    }
}
