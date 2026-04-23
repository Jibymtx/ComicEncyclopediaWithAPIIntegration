using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IFlagService
    {
        Task FlagRecordAsync(string blRecordId, string userId, string reason);
        Task UnflagRecordAsync(string blRecordId);
        Task<IEnumerable<Comic>> GetFlaggedRecordsAsync();
        Task<int> GetFlaggedCountAsync();
        Task<bool> IsFlaggedAsync(string blRecordId);
    }
}
