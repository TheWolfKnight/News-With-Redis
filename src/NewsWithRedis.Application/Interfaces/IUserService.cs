using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Interfaces;

public interface IUserService
{
  Task<User> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
  Task<User> GetuserInfoAsync(string username, CancellationToken cancellationToken = default);
}
