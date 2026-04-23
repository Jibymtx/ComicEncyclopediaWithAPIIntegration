using System.Text.RegularExpressions;

namespace ComicEncyclopedia.Common.Models
{
    public class Comic
    {
        public string BLRecordID { get; set; } = string.Empty;
        public string TypeOfResource { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string OtherTitles { get; set; } = string.Empty;
        public string VariantTitles { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DatesAssociatedWithName { get; set; } = string.Empty;
        public string TypeOfName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AllNames { get; set; } = string.Empty;
        public string OtherNames { get; set; } = string.Empty;
        public string Topics { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Languages { get; set; } = string.Empty;
        public string SeriesTitle { get; set; } = string.Empty;
        public string NumberWithinSeries { get; set; } = string.Empty;
        public string CountryOfPublication { get; set; } = string.Empty;
        public string PlaceOfPublication { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string DateOfPublication { get; set; } = string.Empty;
        public string Edition { get; set; } = string.Empty;
        public string PhysicalDescription { get; set; } = string.Empty;
        public string DeweyClassification { get; set; } = string.Empty;
        public string BLShelfmark { get; set; } = string.Empty;
        public string BNBNumber { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public List<string> VariantISBNs { get; set; } = new();
        public List<string> VariantTitlesList { get; set; } = new();
        public List<int> VariantYears { get; set; } = new();

        public string DisplayISBN => string.IsNullOrWhiteSpace(ISBN) ? "missing" : ISBN;

        public int? PublicationYear
        {
            get
            {
                // check if null
                if (string.IsNullOrWhiteSpace(DateOfPublication)) return null;

                var match = Regex.Match(DateOfPublication, @"\d{4}");
                if (match.Success && int.TryParse(match.Value, out int year))
                {
                    return year;
                }
                return null;
            }
        }

        public string Author => string.IsNullOrWhiteSpace(Name) ? "Unknown Author" : Name;

        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }
        public DateTime? FlaggedDate { get; set; }
        public string? FlaggedBy { get; set; }


        public override string ToString()
        {
            return $"{Title} ({Author}, {DateOfPublication})";
        }
        public Comic Clone()
        {
            return new Comic
            {
                BLRecordID = this.BLRecordID,
                TypeOfResource = this.TypeOfResource,
                ContentType = this.ContentType,
                MaterialType = this.MaterialType,
                Title = this.Title,
                OtherTitles = this.OtherTitles,
                VariantTitles = this.VariantTitles,
                Name = this.Name,
                DatesAssociatedWithName = this.DatesAssociatedWithName,
                TypeOfName = this.TypeOfName,
                Role = this.Role,
                AllNames = this.AllNames,
                OtherNames = this.OtherNames,
                Topics = this.Topics,
                Genre = this.Genre,
                Languages = this.Languages,
                SeriesTitle = this.SeriesTitle,
                NumberWithinSeries = this.NumberWithinSeries,
                CountryOfPublication = this.CountryOfPublication,
                PlaceOfPublication = this.PlaceOfPublication,
                Publisher = this.Publisher,
                DateOfPublication = this.DateOfPublication,
                Edition = this.Edition,
                PhysicalDescription = this.PhysicalDescription,
                DeweyClassification = this.DeweyClassification,
                BLShelfmark = this.BLShelfmark,
                BNBNumber = this.BNBNumber,
                ISBN = this.ISBN,
                Notes = this.Notes,
                VariantISBNs = new List<string>(this.VariantISBNs),
                VariantTitlesList = new List<string>(this.VariantTitlesList),
                VariantYears = new List<int>(this.VariantYears),
                IsFlagged = this.IsFlagged,
                FlagReason = this.FlagReason,
                FlaggedDate = this.FlaggedDate,
                FlaggedBy = this.FlaggedBy
            };
        }
    }
}
