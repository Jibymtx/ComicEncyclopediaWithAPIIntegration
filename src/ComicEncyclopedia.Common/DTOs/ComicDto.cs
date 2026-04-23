namespace ComicEncyclopedia.Common.DTOs
{
    public class ComicDto
    {
        public string BLRecordID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? DateOfPublication { get; set; }
        public int? PublicationYear { get; set; }
        public string DisplayISBN { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string Edition { get; set; } = string.Empty;
        public string PhysicalDescription { get; set; } = string.Empty;
        public string SeriesTitle { get; set; } = string.Empty;
        public string Topics { get; set; } = string.Empty;
        public string PlaceOfPublication { get; set; } = string.Empty;
        public string AllNames { get; set; } = string.Empty;
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }

        public List<string> VariantISBNs { get; set; } = new();
        public List<string> VariantTitles { get; set; } = new();
        public List<int> VariantYears { get; set; } = new();
        public bool HasVariants => VariantISBNs.Count > 0 || VariantTitles.Count > 0 || VariantYears.Count > 0;
    }
}
