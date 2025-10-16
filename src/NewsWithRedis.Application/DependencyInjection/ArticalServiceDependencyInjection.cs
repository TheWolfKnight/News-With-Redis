using System;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NewsWithRedis.Application.DependencyInjection;

public static class ArticalServiceDependencyInjection
{
  public static IServiceCollection AddArticalService(this IServiceCollection @this, string baseAddress)
  {
    @this.AddHttpClient<IArticalService, ArticalService>((serviceProcier, httpClient) =>
    {
      httpClient.BaseAddress = new Uri(baseAddress);
    });

    return @this;
  }
}
