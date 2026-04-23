using ComicEncyclopedia.Common.Models;

namespace ComicEncyclopedia.Tests
{
    internal static class TestData
    {
        public static List<Comic> SampleComics() => new()
        {
            new Comic
            {
                BLRecordID = "001",
                Title = "Batman: Year One",
                Name = "Frank Miller",
                Genre = "Superhero",
                Publisher = "DC Comics",
                DateOfPublication = "1987",
                ISBN = "9780930289331",
                Languages = "English"
            },
            new Comic
            {
                BLRecordID = "002",
                Title = "Watchmen",
                Name = "Alan Moore",
                Genre = "Superhero",
                Publisher = "DC Comics",
                DateOfPublication = "1987",
                ISBN = "9780930289232",
                Languages = "English"
            },
            new Comic
            {
                BLRecordID = "003",
                Title = "Maus",
                Name = "Art Spiegelman",
                Genre = "Biography",
                Publisher = "Pantheon",
                DateOfPublication = "1991",
                ISBN = "9780679406419",
                Languages = "English"
            }
        };
    }
}
