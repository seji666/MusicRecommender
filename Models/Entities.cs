namespace MusicRecommender.Models;

public class FavoriteTrack
{
    public int Id { get; set; }
    public long DeezerId { get; set; }
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string? Album { get; set; }
    public string? CoverUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public string? Link { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

public class RecommendationRecord
{
    public int Id { get; set; }
    public string Mood { get; set; } = "";
    public string? Genre { get; set; }
    public string? Details { get; set; }
    public string ResultJson { get; set; } = "[]";   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
