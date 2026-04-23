using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IComicRepository
    {
        // get all comics
        Task<IEnumerable<Comic>> LoadAllComicsAsync();
        IEnumerable<Comic> GetAllComics();
        IEnumerable<Comic> GetFilteredComics();
        Comic? GetByBLRecordId(string blRecordId);
        void ClearAll();

        int Count { get; }
        int FilteredCount { get; }
        bool IsLoaded { get; }

        Task ReloadDatasetAsync();
        DateTime? LastLoadedDate { get; }
    }
}
