using ComicEncyclopedia.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComicEncyclopedia.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SavedComic> SavedComics { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<ResultLog> ResultLogs { get; set; }
        public DbSet<FlaggedRecord> FlaggedRecords { get; set; }
        public DbSet<DatasetInfo> DatasetInfos { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SavedComic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.BLRecordID }).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.BLRecordID);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.SavedComics)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<SearchLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SearchTime);
                entity.HasIndex(e => e.QueryHash);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.SearchLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<ResultLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BLRecordID).IsUnique();
                entity.HasIndex(e => e.TimesReturned);
            });

            builder.Entity<FlaggedRecord>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BLRecordID);
                entity.HasIndex(e => e.IsResolved);

                entity.HasOne(e => e.FlaggedByUser)
                    .WithMany(u => u.FlaggedRecords)
                    .HasForeignKey(e => e.FlaggedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DatasetInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IsActive);
            });

            SeedRoles(builder);
        }


        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "2",
                    Name = "Staff",
                    NormalizedName = "STAFF"
                },
                new Microsoft.AspNetCore.Identity.IdentityRole
                {
                    Id = "3",
                    Name = "Public",
                    NormalizedName = "PUBLIC"
                }
            );
        }
    }
}
