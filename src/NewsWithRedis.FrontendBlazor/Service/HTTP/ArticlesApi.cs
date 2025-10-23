using NewsWithRedis.FrontendBlazor.Service.Interface;
using System.Text.Json;
using NewsWithRedis.FrontendBlazor.Models;
public class ArticlesApi : IArticlesApi
{
    private readonly HttpClient _http;
    public ArticlesApi(HttpClient http) => _http = http;

    public async Task<(List<Artical> Items, string Source)> GetAllAsync(CancellationToken ct = default)
    {
        using var s = await _http.GetStreamAsync("/api/articles", ct);
        using var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);

        var root = doc.RootElement;
        var source = root.GetProperty("source").GetString() ?? "";
        var items = new List<Artical>();

        foreach (var el in root.GetProperty("data").EnumerateArray())
        {
            items.Add(new Artical
            {
                Id = el.TryGetProperty("id", out var idEl) ? idEl.GetInt32() : 0,
                Title = el.GetProperty("title").GetString()!,
                AuthorId = el.GetProperty("authorId").GetInt32(),
                Content = el.GetProperty("content").GetString()!,
                CreatedAt = el.GetProperty("createdAt").GetDateTime()
            });
        }
        return (items, source);
    }


    public async Task<(Artical Item, string Source)> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var s = await _http.GetStreamAsync($"/api/articles/{id}", ct);
        using var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);

        var root = doc.RootElement;
        var source = root.GetProperty("source").GetString() ?? "";
        var parent = root.GetProperty("data").GetProperty("parent");

        var item = new Artical
        {
            Id = id,
            Title = parent.GetProperty("title").GetString()!,
            AuthorId = parent.GetProperty("authorId").GetInt32(),
            Content = parent.GetProperty("content").GetString()!,
            CreatedAt = parent.GetProperty("createdAt").GetDateTime()
        };
        return (item, source);
    }

    Task<(List<Artical> Items, string Source)> IArticlesApi.GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    Task<(Artical Item, string Source)> IArticlesApi.GetByIdAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
