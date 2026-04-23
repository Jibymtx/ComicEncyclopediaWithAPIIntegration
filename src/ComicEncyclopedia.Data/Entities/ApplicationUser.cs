using Microsoft.AspNetCore.Identity;

namespace ComicEncyclopedia.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<SavedComic> SavedComics { get; set; } = new List<SavedComic>();
        public virtual ICollection<SearchLog> SearchLogs { get; set; } = new List<SearchLog>();
        public virtual ICollection<FlaggedRecord> FlaggedRecords { get; set; } = new List<FlaggedRecord>();
    }
}
