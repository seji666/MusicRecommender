using Microsoft.AspNetCore.Mvc;
using MusicRecommender.Models;
using MusicRecommender.Services;

namespace MusicRecommender.Controllers;

public class HomeController : Controller
{
    private readonly IDeezerService _deezer;

    public HomeController(IDeezerService deezer) => _deezer = deezer;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var vm = new TrackListViewModel();
        try
        {
            vm.Tracks = await _deezer.GetChartAsync(12, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            vm.Error = "Не може да се зареди класацията от Deezer. Опитайте отново по-късно.";
        }
        return View(vm);
    }
}
