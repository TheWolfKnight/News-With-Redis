using System.Threading;
using System.Threading.Tasks;

namespace NewsWithRedis.Application.Interfaces;

public interface ICacheService
{
  Task DeleteCacheAsync(CancellationToken cancellationToken = default);
}
