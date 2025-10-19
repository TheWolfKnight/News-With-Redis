using System;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NewsWithRedis.Application.DependencyInjection;

public static class ArticleServiceDependencyInjection
{
  public static IServiceCollection AddArticleService(this IServiceCollection @this, string baseAddress)
  {
    @this.AddHttpClient<IArticleService, ArticleService>((serviceProcier, httpClient) =>
    {
      httpClient.BaseAddress = new Uri(baseAddress);
    });

    return @this;
  }
}
