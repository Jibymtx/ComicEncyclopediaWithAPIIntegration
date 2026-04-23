using ComicEncyclopedia.Common.DTOs;
using ComicEncyclopedia.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComicEncyclopedia.Web.ViewModels
{
    public class ComicListViewModel
    {
        public List<ComicDto> Comics { get; set; } = new();
        public int TotalCount { get; set; }
        public int DisplayedCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public List<string> AvailableGenres { get; set; } = new();
        public List<string> AvailableAuthors { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<string> AvailableLanguages { get; set; } = new();

        public string? SearchTitle { get; set; }
        public string? SelectedGenre { get; set; }
        public string? SelectedAuthor { get; set; }
        public int? SelectedYear { get; set; }
        public SortOrder SortOrder { get; set; } = SortOrder.TitleAscending;
        public GroupBy GroupBy { get; set; } = GroupBy.None;

        public Dictionary<string, List<ComicDto>>? GroupedByAuthor { get; set; }
        public Dictionary<int, List<ComicDto>>? GroupedByYear { get; set; }

        public bool IsDataLoaded { get; set; }
        public DateTime? LastLoadedDate { get; set; }
        public string? StatusMessage { get; set; }
    }

    public class AdvancedSearchViewModel
    {
        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Author")]
        public string? Author { get; set; }

        [Display(Name = "Publication Year")]
        public int? Year { get; set; }

        [Display(Name = "Genre")]
        public string? Genre { get; set; }

        [Display(Name = "Edition")]
        public string? Edition { get; set; }

        [Display(Name = "Language")]
        public string? Language { get; set; }

        [Display(Name = "Name Type")]
        public string? NameType { get; set; }

        public List<string> AvailableGenres { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public List<string> AvailableLanguages { get; set; } = new();
        public List<string> AvailableNameTypes { get; set; } = new();
    }


    public class ComicDetailsViewModel
    {
        public ComicDto Comic { get; set; } = null!;
        public bool IsInSearchList { get; set; }
        public bool CanFlag { get; set; }
        public bool IsLoggedIn { get; set; }

        public List<string> FormattedAllNames { get; set; } = new();
        public List<string> FormattedTopics { get; set; } = new();

        // fetch cover image
        public string? CoverUrl { get; set; }
    }

    public class SearchListViewModel
    {
        public List<ComicDto> SavedComics { get; set; } = new();
        public int Count { get; set; }
        public string? Message { get; set; }
    }

    public class ReportsViewModel
    {
        public ReportDto Report { get; set; } = new();
        public int TotalSearches { get; set; }
        public int FlaggedRecordsCount { get; set; }
        public DateTime? LastDatasetUpdate { get; set; }
        public bool IsDatasetUpdateAvailable { get; set; }
    }

    public class FlagRecordViewModel
    {
        public string BLRecordID { get; set; } = string.Empty;
        public string ComicTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a reason for flagging this record.")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        [Display(Name = "Reason for Flagging")]
        public string Reason { get; set; } = string.Empty;
    }

    public class HomeViewModel
    {
        public bool IsDataLoaded { get; set; }
        public int TotalComics { get; set; }
        public int FilteredComics { get; set; }
        public DateTime? LastLoadedDate { get; set; }
        public List<ComicDto> RecentlyViewed { get; set; } = new();
        public List<ComicDto> FeaturedComics { get; set; } = new();
        public string WelcomeMessage { get; set; } = "Welcome to Fantasy B'zaar Comic Encyclopedia";
    }
}
