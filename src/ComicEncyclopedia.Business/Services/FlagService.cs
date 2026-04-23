using ComicEncyclopedia.Common.Interfaces;
using ComicEncyclopedia.Common.Models;
using ComicEncyclopedia.Data.Context;
using ComicEncyclopedia.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ComicEncyclopedia.Business.Services
{
    public class FlagService : IFlagService
    {
        private readonly ApplicationDbContext _context;
        private readonly IComicRepository _comicRepository;

        public FlagService(ApplicationDbContext context, IComicRepository comicRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _comicRepository = comicRepository ?? throw new ArgumentNullException(nameof(comicRepository));
        }

        public async Task FlagRecordAsync(string blRecordId, string userId, string reason)
        {
            // check if null
            if (string.IsNullOrWhiteSpace(blRecordId))
                throw new ArgumentNullException(nameof(blRecordId));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentNullException(nameof(reason));

            var existingFlag = await _context.FlaggedRecords
                .FirstOrDefaultAsync(f => f.BLRecordID == blRecordId && !f.IsResolved);

            if (existingFlag != null)
            {
                existingFlag.Reason = reason;
                existingFlag.FlaggedByUserId = userId;
                existingFlag.FlaggedDate = DateTime.UtcNow;
            }
            else
            {
                var comic = _comicRepository.GetByBLRecordId(blRecordId);

                var flag = new FlaggedRecord
                {
                    BLRecordID = blRecordId,
                    Title = comic?.Title ?? "Unknown",
                    FlaggedByUserId = userId,
                    Reason = reason,
                    FlaggedDate = DateTime.UtcNow,
                    IsResolved = false
                };

                _context.FlaggedRecords.Add(flag);
            }

            await _context.SaveChangesAsync();

            var comicInMemory = _comicRepository.GetByBLRecordId(blRecordId);
            if (comicInMemory != null)
            {
                comicInMemory.IsFlagged = true;
                comicInMemory.FlagReason = reason;
                comicInMemory.FlaggedDate = DateTime.UtcNow;
                comicInMemory.FlaggedBy = userId;
            }
        }

        public async Task UnflagRecordAsync(string blRecordId)
        {
            if (string.IsNullOrWhiteSpace(blRecordId))
                throw new ArgumentNullException(nameof(blRecordId));

            var flag = await _context.FlaggedRecords
                .FirstOrDefaultAsync(f => f.BLRecordID == blRecordId && !f.IsResolved);

            if (flag != null)
            {
                flag.IsResolved = true;
                flag.ResolvedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var comicInMemory = _comicRepository.GetByBLRecordId(blRecordId);
            if (comicInMemory != null)
            {
                comicInMemory.IsFlagged = false;
                comicInMemory.FlagReason = null;
                comicInMemory.FlaggedDate = null;
                comicInMemory.FlaggedBy = null;
            }
        }


        public async Task<IEnumerable<Comic>> GetFlaggedRecordsAsync()
        {
            var flaggedIds = await _context.FlaggedRecords
                .Where(f => !f.IsResolved)
                .Select(f => f.BLRecordID)
                .ToListAsync();

            var flaggedComics = new List<Comic>();
            foreach (var id in flaggedIds)
            {
                var comic = _comicRepository.GetByBLRecordId(id);
                if (comic != null)
                {
                    flaggedComics.Add(comic);
                }
            }

            return flaggedComics;
        }

        public async Task<int> GetFlaggedCountAsync()
        {
            return await _context.FlaggedRecords
                .CountAsync(f => !f.IsResolved);
        }

        public async Task<bool> IsFlaggedAsync(string blRecordId)
        {
            if (string.IsNullOrWhiteSpace(blRecordId))
                return false;

            return await _context.FlaggedRecords
                .AnyAsync(f => f.BLRecordID == blRecordId && !f.IsResolved);
        }
    }
}
