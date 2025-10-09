
namespace NewsWithRedis.Common.Models;

public class Artical
{
  public required string Title {get; set; }
  public required int AuthorId { get; set; }
  public required string ArticalText { get; set; }
}
