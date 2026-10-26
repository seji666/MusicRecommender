using Microsoft.EntityFrameworkCore;
using MusicRecommender.Models;

namespace MusicRecommender.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FavoriteTrack> Favorites => Set<FavoriteTrack>();
    public DbSet<RecommendationRecord> Recommendations => Set<RecommendationRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<FavoriteTrack>().HasIndex(f => f.DeezerId).IsUnique();
    }
}
