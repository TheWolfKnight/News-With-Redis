using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Services;

internal class UserService : IUserService
{
  private IHttpClientFactory _factory;

  public UserService(IHttpClientFactory factory)
  {
    _factory = factory;
  }

  public async Task<IEnumerable<User>> GetUsersAsync(CancellationToken cancellationToken = default)
  {
    string url = $"api/user";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      IEnumerable<User> users = await JsonSerializer.DeserializeAsync<IEnumerable<User>>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken) ?? [];

      return users;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR Application::UserService::GetUserInfoAsync] message: {e.Message}");
      throw e;
    }
  }

  public async Task<User> GetUserInfoAsync (int userId, CancellationToken cancellationToken = default)
  {
    string url = $"api/user/{userId}";

    try
    {
      HttpClient client = _factory.CreateClient();
      HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
      response.EnsureSuccessStatusCode();

      User? user = await JsonSerializer.DeserializeAsync<User>(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

      if (user is null)
        throw new InvalidOperationException("Could not deserialize user from response");

      return user;
    }
    catch (Exception e)
    {
      Console.WriteLine($" [ERROR Application::UserService::GetUserInfoAsync] message: {e.Message}");
      throw e;
    }
  }

  public Task<User> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
