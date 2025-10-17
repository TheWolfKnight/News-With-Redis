using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Services;

internal class ArticalService : IArticalService
{
  private readonly IHttpClientFactory _factory;

  public ArticalService(IHttpClientFactory factory)
  {
    _factory = factory;
  }

  public async Task<Artical> GetArticalAsync(string title, CancellationToken cancellationToken = default)
  {
    string url = $"api/article/{title}";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      Artical? artical = await JsonSerializer.DeserializeAsync<Artical>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
      if (artical is null)
        throw new InvalidOperationException("Could not deserialize Artical response");

      return artical;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR: Application::ArticalService::GetArticalAsync] message: {e.Message}");
      throw e;
    }
  }

  public async Task<IEnumerable<Artical>> GetArticalsAsync(CancellationToken cancellationToken = default)
  {
    string url = "api/article";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      IEnumerable<Artical> articals = await JsonSerializer.DeserializeAsync<IEnumerable<Artical>>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken) ?? [];

      return articals;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR Application::ArticalService::GetArticals] message: {e.Message}");
      throw e;
    }
  }
}
