using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using ComicEncyclopedia.Data.Context;
using ComicEncyclopedia.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ComicEncyclopedia.Business.Services
{
    public class SearchListService : ISearchListService
    {
        private readonly ApplicationDbContext _context;

        public SearchListService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddToSearchListAsync(string userId, Comic comic)
        {
            // check if null
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (comic == null)
                throw new ArgumentNullException(nameof(comic));

            var exists = await _context.SavedComics
                .AnyAsync(s => s.UserId == userId && s.BLRecordID == comic.BLRecordID);

            if (!exists)
            {
                var savedComic = new SavedComic
                {
                    UserId = userId,
                    BLRecordID = comic.BLRecordID,
                    Title = comic.Title,
                    Author = comic.Author,
                    Genre = comic.Genre,
                    DateOfPublication = comic.DateOfPublication,
                    ISBN = comic.ISBN,
                    SavedDate = DateTime.UtcNow,
                    SerializedComicData = JsonSerializer.Serialize(comic)
                };

                _context.SavedComics.Add(savedComic);
                await _context.SaveChangesAsync();
            }
        }


        public async Task RemoveFromSearchListAsync(string userId, string blRecordId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(blRecordId))
                throw new ArgumentNullException(nameof(blRecordId));

            var savedComic = await _context.SavedComics
                .FirstOrDefaultAsync(s => s.UserId == userId && s.BLRecordID == blRecordId);

            if (savedComic != null)
            {
                _context.SavedComics.Remove(savedComic);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Comic>> GetSearchListAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<Comic>();

            var savedComics = await _context.SavedComics
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedDate)
                .ToListAsync();

            var comics = new List<Comic>();

            foreach (var saved in savedComics)
            {
                if (!string.IsNullOrWhiteSpace(saved.SerializedComicData))
                {
                    try
                    {
                        var comic = JsonSerializer.Deserialize<Comic>(saved.SerializedComicData);
                        if (comic != null)
                        {
                            comics.Add(comic);
                        }
                    }
                    catch
                    {
                        comics.Add(new Comic
                        {
                            BLRecordID = saved.BLRecordID,
                            Title = saved.Title,
                            Name = saved.Author,
                            Genre = saved.Genre ?? string.Empty,
                            DateOfPublication = saved.DateOfPublication ?? string.Empty,
                            ISBN = saved.ISBN ?? string.Empty
                        });
                    }
                }
            }

            return comics;
        }

        public async Task ClearSearchListAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            var savedComics = await _context.SavedComics
                .Where(s => s.UserId == userId)
                .ToListAsync();

            _context.SavedComics.RemoveRange(savedComics);
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            return await _context.SavedComics
                .CountAsync(s => s.UserId == userId);
        }

        public async Task<bool> ContainsAsync(string userId, string blRecordId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(blRecordId))
                return false;

            return await _context.SavedComics
                .AnyAsync(s => s.UserId == userId && s.BLRecordID == blRecordId);
        }
    }
}
