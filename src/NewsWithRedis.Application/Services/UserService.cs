using System;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Services;

internal class UserService : IUserService
{


  public Task<User> GetuserInfoAsync(string username, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<User> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
