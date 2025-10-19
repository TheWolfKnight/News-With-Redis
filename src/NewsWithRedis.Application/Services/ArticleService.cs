using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Services;

internal class ArticleService : IArticleService
{
  private sealed record ResponseWithSource<TData>(TData Data, string Source);

  private readonly IHttpClientFactory _factory;

  public ArticleService(IHttpClientFactory factory)
  {
    _factory = factory;
  }

  public async Task<Article> GetArticleAsync(int articleId, CancellationToken cancellationToken = default)
  {
    string url = $"api/article/{articleId}";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      ResponseWithSource<Article>? article = await JsonSerializer.DeserializeAsync<ResponseWithSource<Article>>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

      if (article is null)
        throw new InvalidOperationException("Could not deserialize Artical response");

      Console.WriteLine($" [INFO: Application::ArticleService::GetArticleAsync] article source: {article.Source}");

      return article.Data;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR: Application::ArticleService::GetArticleAsync] message: {e.Message}");
      throw e;
    }
  }

  public async Task<IEnumerable<Article>> GetArticlesAsync(CancellationToken cancellationToken = default)
  {
    string url = "api/article";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      ResponseWithSource<IEnumerable<Article>>? articles = await JsonSerializer.DeserializeAsync<ResponseWithSource<IEnumerable<Article>>>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

      if (articles is null)
        throw new InvalidOperationException("Could not deserialize articles from response");

      Console.WriteLine($" [INFO: Application::ArticleService::GetArticlesAsync] articles source: {articles.Source}");

      return articles.Data;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR: Application::ArticleService::GetArticles] message: {e.Message}");
      throw e;
    }
  }
}
