using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Interfaces;

public interface IUserService
{
  Task<User> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
  Task<User> GetUserInfoAsync (int userId, CancellationToken cancellationToken = default);
  Task<IEnumerable<User>> GetUsersAsync(CancellationToken cancellationToken = default);
}
