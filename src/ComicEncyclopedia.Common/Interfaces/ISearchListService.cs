using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface ISearchListService
    {
        Task AddToSearchListAsync(string userId, Comic comic);
        Task RemoveFromSearchListAsync(string userId, string blRecordId);
        Task<IEnumerable<Comic>> GetSearchListAsync(string userId);
        Task ClearSearchListAsync(string userId);
        Task<int> GetCountAsync(string userId);
        Task<bool> ContainsAsync(string userId, string blRecordId);
    }
}
