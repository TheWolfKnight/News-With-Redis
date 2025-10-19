using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Interfaces;

public interface IArticleService
{
  Task<IEnumerable<Article>> GetArticalsAsync(CancellationToken cancellationToken = default);
  Task<Article> GetArticalAsync(int articleId, CancellationToken cancellationToken = default);
}
