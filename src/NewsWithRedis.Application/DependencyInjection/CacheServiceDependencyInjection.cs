using System;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NewsWithRedis.Application.DependencyInjection;

public static class CacheServiceDependencyInjection
{
  public static IServiceCollection AddCacheService(this IServiceCollection @this, string baseAddress)
  {
    @this.AddHttpClient<ICacheService, CacheService>(client =>
    {
      client.BaseAddress = new Uri(baseAddress);
    });

      return @this;
  }
}
