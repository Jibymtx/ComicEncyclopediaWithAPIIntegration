using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Data.Context;
using ComicEncyclopedia.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ComicEncyclopedia.Business.Services
{
    public class DatasetUpdateService : IDatasetUpdateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IComicRepository _comicRepository;
        private readonly ILogger<DatasetUpdateService> _logger;
        private readonly HttpClient _httpClient;

        private bool _isUpdating = false;
        private DateTime? _lastUpdateDate;
        private DateTime? _lastCheckDate;

        public string DatasetUrl => "https://www.bl.uk/bibliographic/downloads/ComicsResearcherFormat_202204_csv.zip";

        public DatasetUpdateService(
            ApplicationDbContext context,
            IComicRepository comicRepository,
            ILogger<DatasetUpdateService> logger,
            HttpClient httpClient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _comicRepository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public bool IsUpdating => _isUpdating;
        public DateTime? LastUpdateDate => _lastUpdateDate;
        public DateTime? LastCheckDate => _lastCheckDate;

        public async Task<bool> CheckForUpdatesAsync()
        {
            try
            {
                _lastCheckDate = DateTime.UtcNow;

                var currentDataset = await _context.DatasetInfos
                    .Where(d => d.IsActive)
                    .OrderByDescending(d => d.LastDownloadDate)
                    .FirstOrDefaultAsync();

                var request = new HttpRequestMessage(HttpMethod.Head, DatasetUrl);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // return error
                    _logger.LogWarning("Unable to check for dataset updates. Status: {StatusCode}", response.StatusCode);
                    return false;
                }

                if (response.Content.Headers.LastModified.HasValue && currentDataset != null)
                {
                    var remoteModified = response.Content.Headers.LastModified.Value.UtcDateTime;
                    if (remoteModified > currentDataset.LastDownloadDate)
                    {
                        _logger.LogInformation("Dataset update available. Remote: {RemoteDate}, Local: {LocalDate}",
                            remoteModified, currentDataset.LastDownloadDate);
                        return true;
                    }
                }

                if (currentDataset == null)
                {
                    return true;
                }

                _logger.LogInformation("No dataset updates available.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for dataset updates");
                return false;
            }
        }


        public async Task<bool> UpdateDatasetAsync()
        {
            if (_isUpdating)
            {
                _logger.LogWarning("Dataset update already in progress");
                return false;
            }

            try
            {
                _isUpdating = true;
                _logger.LogInformation("Starting dataset update...");

                var response = await _httpClient.GetAsync(DatasetUrl);
                if (!response.IsSuccessStatusCode)
                {
                    // return error
                    _logger.LogError("Failed to download dataset. Status: {StatusCode}", response.StatusCode);
                    return false;
                }

                var zipContent = await response.Content.ReadAsByteArrayAsync();
                var fileHash = ComputeHash(zipContent);

                var currentDataset = await _context.DatasetInfos
                    .Where(d => d.IsActive)
                    .OrderByDescending(d => d.LastDownloadDate)
                    .FirstOrDefaultAsync();

                if (currentDataset?.FileHash == fileHash)
                {
                    _logger.LogInformation("Dataset file unchanged, skipping update.");
                    return true;
                }

                _logger.LogInformation("Processing new dataset...");

                var tempPath = Path.Combine(Path.GetTempPath(), "comic_dataset_" + Guid.NewGuid());
                Directory.CreateDirectory(tempPath);

                try
                {
                    var zipPath = Path.Combine(tempPath, "dataset.zip");
                    await File.WriteAllBytesAsync(zipPath, zipContent);
                    System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, tempPath);

                    var csvFiles = Directory.GetFiles(tempPath, "names.csv", SearchOption.AllDirectories);
                    if (csvFiles.Length == 0)
                    {
                        _logger.LogError("names.csv not found in dataset");
                        return false;
                    }

                    var csvPath = csvFiles[0];

                    if (_comicRepository is Data.Repositories.ComicRepository concreteRepo)
                    {
                        concreteRepo.SetFilePath(csvPath);
                        await _comicRepository.LoadAllComicsAsync();
                    }

                    if (currentDataset != null)
                    {
                        currentDataset.IsActive = false;
                    }

                    var newDataset = new DatasetInfo
                    {
                        SourceUrl = DatasetUrl,
                        FileHash = fileHash,
                        LastDownloadDate = DateTime.UtcNow,
                        LastCheckDate = DateTime.UtcNow,
                        RecordCount = _comicRepository.Count,
                        FilteredRecordCount = _comicRepository.FilteredCount,
                        IsActive = true
                    };

                    _context.DatasetInfos.Add(newDataset);
                    await _context.SaveChangesAsync();

                    _lastUpdateDate = DateTime.UtcNow;
                    _logger.LogInformation("Dataset update complete. Loaded {Count} comics ({FilteredCount} filtered)",
                        _comicRepository.Count, _comicRepository.FilteredCount);

                    return true;
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up temp directory: {Path}", tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dataset");
                return false;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private string ComputeHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
