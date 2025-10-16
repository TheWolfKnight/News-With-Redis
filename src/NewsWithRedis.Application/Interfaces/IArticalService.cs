using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Interfaces;

public interface IArticalService
{
  Task<IEnumerable<Artical>> GetArticalsAsync(CancellationToken cancellationToken = default);
  Task<Artical> GetArticalAsync(string title, CancellationToken cancellationToken = default);
}
