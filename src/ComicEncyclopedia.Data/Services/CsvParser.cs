using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using System.Text;

namespace ComicEncyclopedia.Data.Services
{
    public class CsvParser : ICsvParser
    {
        private readonly ITextSanitizer _textSanitizer;
        private char _delimiter = ',';

        public CsvParser(ITextSanitizer textSanitizer)
        {
            _textSanitizer = textSanitizer ?? throw new ArgumentNullException(nameof(textSanitizer));
        }

        public async Task<IEnumerable<Comic>> ParseFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV file not found: {filePath}");

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: true);
            return await ParseStreamAsync(stream);
        }

        public async Task<IEnumerable<Comic>> ParseStreamAsync(Stream stream)
        {
            var comics = new List<Comic>();
            string[]? headers = null;
            int lineNumber = 0;
            int successCount = 0;
            int errorCount = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 65536);

            var headerLine = await reader.ReadLineAsync();
            lineNumber++;

            if (string.IsNullOrEmpty(headerLine))
                throw new InvalidDataException("CSV file is empty or has no headers");

            _delimiter = DetectDelimiter(headerLine);

            headers = ParseLine(headerLine);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var comic = ParseLineToComic(line, headers);
                    if (comic != null && !string.IsNullOrWhiteSpace(comic.BLRecordID))
                    {
                        comics.Add(comic);
                        successCount++;
                    }
                }
                catch
                {
                    errorCount++;
                }
            }

            return comics;
        }

        private char DetectDelimiter(string headerLine)
        {
            int tabCount = headerLine.Count(c => c == '\t');
            int commaCount = headerLine.Count(c => c == ',');

            return tabCount > commaCount ? '\t' : ',';
        }

        public Comic? ParseLine(string line, string[] headers)
        {
            return ParseLineToComic(line, headers);
        }


        private Comic? ParseLineToComic(string line, string[] headers)
        {
            try
            {
                var values = ParseLine(line);

                // check if null
                if (values.Length == 0)
                    return null;

                var comic = new Comic();

                for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
                {
                    var header = headers[i].Trim().ToLowerInvariant();
                    var rawValue = values[i];
                    var value = _textSanitizer.SanitizeText(rawValue);

                    MapValueToComic(comic, header, value);
                }

                return comic;
            }
            catch
            {
                return null;
            }
        }
        private void MapValueToComic(Comic comic, string header, string value)
        {
            switch (header)
            {
                case "bl record id":
                    comic.BLRecordID = value;
                    break;
                case "type of resource":
                    comic.TypeOfResource = value;
                    break;
                case "content type":
                    comic.ContentType = value;
                    break;
                case "material type":
                    comic.MaterialType = value;
                    break;
                case "title":
                    comic.Title = value;
                    break;
                case "other titles":
                    comic.OtherTitles = value;
                    break;
                case "variant titles":
                    comic.VariantTitles = value;
                    break;
                case "name":
                    comic.Name = value;
                    break;
                case "dates associated with name":
                    comic.DatesAssociatedWithName = value;
                    break;
                case "type of name":
                    comic.TypeOfName = value;
                    break;
                case "role":
                    comic.Role = value;
                    break;
                case "all names":
                    comic.AllNames = value;
                    break;
                case "other names":
                    comic.OtherNames = value;
                    break;
                case "topics":
                    comic.Topics = value;
                    break;
                case "genre":
                    comic.Genre = value;
                    break;
                case "languages":
                    comic.Languages = value;
                    break;
                case "series title":
                    comic.SeriesTitle = value;
                    break;
                case "number within series":
                    comic.NumberWithinSeries = value;
                    break;
                case "country of publication":
                    comic.CountryOfPublication = value;
                    break;
                case "place of publication":
                    comic.PlaceOfPublication = value;
                    break;
                case "publisher":
                    comic.Publisher = value;
                    break;
                case "date of publication":
                    comic.DateOfPublication = value;
                    break;
                case "edition":
                    comic.Edition = value;
                    break;
                case "physical description":
                    comic.PhysicalDescription = value;
                    break;
                case "dewey classification":
                    comic.DeweyClassification = value;
                    break;
                case "bl shelfmark":
                    comic.BLShelfmark = value;
                    break;
                case "bnb number":
                    comic.BNBNumber = value;
                    break;
                case "isbn":
                    comic.ISBN = _textSanitizer.FormatISBN(value);
                    break;
                case "notes":
                    comic.Notes = value;
                    break;
            }
        }


        private string[] ParseLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == _delimiter && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString());
            return values.ToArray();
        }
    }
}
