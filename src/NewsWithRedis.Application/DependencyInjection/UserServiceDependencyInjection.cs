using System;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace NewsWithRedis.Application.DependencyInjection;

public static class UserServiceDependencyInjection
{
  public static IServiceCollection AddUserService(this IServiceCollection @this, string baseAddress)
  {
    @this.AddHttpClient<IUserService, UserService>((serviceProvider, httpClient) =>
    {
      httpClient.BaseAddress = new Uri (baseAddress);
    });

    return @this;
  }
}
