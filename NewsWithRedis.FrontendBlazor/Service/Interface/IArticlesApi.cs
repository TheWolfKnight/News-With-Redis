using System.Threading;
using NewsWithRedis.FrontendBlazor.Models;
namespace NewsWithRedis.FrontendBlazor.Service.Interface
{
    public interface IArticlesApi
    {
        Task<(List<Artical> Items, string Source)> GetAllAsync(CancellationToken ct = default);
        Task<(Artical Item, string Source)> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
