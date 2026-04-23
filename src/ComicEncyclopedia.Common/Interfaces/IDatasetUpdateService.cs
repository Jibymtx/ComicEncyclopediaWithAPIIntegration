namespace ComicEncyclopedia.Common.Interfaces
{
    public interface IDatasetUpdateService
    {
        Task<bool> CheckForUpdatesAsync();
        Task<bool> UpdateDatasetAsync();
        DateTime? LastUpdateDate { get; }
        DateTime? LastCheckDate { get; }
        bool IsUpdating { get; }
        string DatasetUrl { get; }
    }
}
