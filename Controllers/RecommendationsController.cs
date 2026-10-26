using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicRecommender.Data;
using MusicRecommender.Models;
using MusicRecommender.Services;

namespace MusicRecommender.Controllers;

public class RecommendationsController : Controller
{
    private readonly ILlmService _llm;
    private readonly IDeezerService _deezer;
    private readonly AppDbContext _db;

    public RecommendationsController(ILlmService llm, IDeezerService deezer, AppDbContext db)
    {
        _llm = llm;
        _deezer = deezer;
        _db = db;
    }

    [HttpGet]
    public IActionResult Index() => View(new RecommendationViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        [Bind(Prefix = "Request")] RecommendationRequest request, CancellationToken ct)
    {
        var vm = new RecommendationViewModel { Request = request };
        if (!ModelState.IsValid) return View(vm);

        try
        {
            
            var favorites = new List<string>();
            if (request.UseFavorites)
            {
                favorites = await _db.Favorites
                    .OrderByDescending(f => f.AddedAt)
                    .Take(10)
                    .Select(f => f.Artist + " - " + f.Title)
                    .ToListAsync(ct);
            }

            
            var suggestions = await _llm.GetSuggestionsAsync(request, favorites, ct);
            if (suggestions.Count == 0)
            {
                vm.Error = "Моделът не върна препоръки. Опитайте с друго описание.";
                return View(vm);
            }

            
            var tasks = suggestions.Select(async s => new RecommendationItem
            {
                Suggestion = s,
                Track = await _deezer.FindTrackAsync(s.Artist, s.Title, ct)
            });
            vm.Results = (await Task.WhenAll(tasks)).ToList();

            
            _db.Recommendations.Add(new RecommendationRecord
            {
                Mood = request.Mood,
                Genre = request.Genre,
                Details = request.Details,
                ResultJson = JsonSerializer.Serialize(suggestions)
            });
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                      or InvalidOperationException or JsonException or KeyNotFoundException)
        {
            vm.Error = "Грешка при комуникация с езиковия модел. Проверете дали е стартиран " +
                       "и дали настройките в appsettings.json (раздел Llm) са верни. " +
                       $"Детайли: {ex.Message}";
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> History(CancellationToken ct)
    {
        var records = await _db.Recommendations
            .OrderByDescending(r => r.CreatedAt)
            .Take(30)
            .ToListAsync(ct);

        var items = records.Select(r => new HistoryItem
        {
            Record = r,
            Suggestions = JsonSerializer.Deserialize<List<AiSuggestion>>(r.ResultJson)
                          ?? new List<AiSuggestion>()
        }).ToList();

        return View(items);
    }
}
