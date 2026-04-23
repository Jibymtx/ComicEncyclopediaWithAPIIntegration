using ComicEncyclopedia.Common.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace ComicEncyclopedia.Business.Helpers
{
    public class TextSanitizer : ITextSanitizer
    {
        public string SanitizeText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder();

            foreach (char c in input)
            {
                if (c >= 32 && c < 127)
                {
                    sb.Append(c);
                }
                else if (c == '\n' || c == '\r' || c == '\t')
                {
                    sb.Append(c);
                }
                else if (c >= 128)
                {
                    sb.Append(GetAsciiEquivalent(c));
                }
            }

            return sb.ToString().Trim();
        }

        private string GetAsciiEquivalent(char c)
        {
            return c switch
            {
                'é' => "e",
                'è' => "e",
                'ê' => "e",
                'ë' => "e",
                'á' => "a",
                'à' => "a",
                'â' => "a",
                'ä' => "a",
                'ã' => "a",
                'ü' => "u",
                'ú' => "u",
                'ù' => "u",
                'û' => "u",
                'ö' => "o",
                'ó' => "o",
                'ò' => "o",
                'ô' => "o",
                'õ' => "o",
                'í' => "i",
                'ì' => "i",
                'î' => "i",
                'ï' => "i",
                'ñ' => "n",
                'ç' => "c",
                'ß' => "ss",
                '£' => "GBP",
                '©' => "(c)",
                '®' => "(R)",
                '’' => "'",
                '‘' => "'",
                '“' => "\"",
                '”' => "\"",
                '–' => "-",
                '—' => "-",
                '…' => "...",
                '™' => "(TM)",
                '°' => " degrees",
                _ => ""
            };
        }


        public string FormatSemicolonValues(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var values = ParseMultipleValues(input);
            var formattedValues = values.Select(v => $"[{fieldName}]:[{SanitizeText(v)}]");

            return string.Join(Environment.NewLine, formattedValues);
        }

        public IEnumerable<string> ParseMultipleValues(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Enumerable.Empty<string>();

            return input.Split(';', StringSplitOptions.RemoveEmptyEntries)
                       .Select(v => v.Trim())
                       .Where(v => !string.IsNullOrEmpty(v));
        }

        public string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var noScripts = Regex.Replace(
                input,
                @"<script[^>]*>.*?</script>",
                string.Empty,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return Regex.Replace(noScripts, @"<[^>]+>", string.Empty);
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public string FormatISBN(string input)
        {
            // check if null
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var trimmed = input.Trim();

            if (trimmed.Contains('E', StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (double.TryParse(trimmed, out double value))
                    {
                        long isbn = (long)value;
                        return isbn.ToString();
                    }
                }
                catch
                {
                    return SanitizeText(trimmed);
                }
            }

            var digitsOnly = Regex.Replace(trimmed, @"[^\dX]", "", RegexOptions.IgnoreCase);

            if (digitsOnly.Length == 10 || digitsOnly.Length == 13)
            {
                return digitsOnly;
            }

            return SanitizeText(trimmed);
        }
    }
}
