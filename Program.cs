using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MusicRecommender.Data;
using MusicRecommender.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);


builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddHttpClient<IDeezerService, DeezerService>(c =>
{
    c.BaseAddress = new Uri("https://api.deezer.com/");
    c.Timeout = TimeSpan.FromSeconds(15);
});


builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("Llm"));
builder.Services.AddHttpClient<ILlmService, LlmService>((sp, c) =>
{
    var opt = sp.GetRequiredService<IOptions<LlmOptions>>().Value;
    c.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
    c.Timeout = TimeSpan.FromMinutes(3);
    if (!string.IsNullOrWhiteSpace(opt.ApiKey))
        c.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", opt.ApiKey);
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
