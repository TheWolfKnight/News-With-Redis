using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsWithRedis.Application.Interfaces;
using NewsWithRedis.Common.Models;

namespace NewsWithRedis.Application.Services;

internal class ArticalService : IArticalService
{
  public Task<Artical> GetArticalAsync(string title, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  public Task<IEnumerable<Artical>> GetArticalsAsync(CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
