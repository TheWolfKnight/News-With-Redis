using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Interfaces;

public interface IArticleService
{
  Task<IEnumerable<Article>> GetArticlesAsync(CancellationToken cancellationToken = default);
  Task<Article> GetArticleAsync(int articleId, CancellationToken cancellationToken = default);
}
