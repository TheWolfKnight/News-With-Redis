using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Application.Interfaces;

namespace NewsWithRedis.Application.Services;

public class CacheService : ICacheService
{
  private IHttpClientFactory _factory;

  public CacheService(IHttpClientFactory factory)
  {
    _factory = factory;
  }

  public async Task DeleteCacheAsync(CancellationToken cancellationToken = default)
  {
    string url = "api/cache";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.DeleteAsync(url);
      response.EnsureSuccessStatusCode();
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR: Application::CacheService::ResetCacheAsync] message: {e.Message}");

      throw e;
    }
  }
}
