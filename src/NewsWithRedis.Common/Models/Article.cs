using System;

namespace NewsWithRedis.Common.Models;

public class Article
{
  public required int Id { get; set; }
  public required string Title { get; set; }
  public required int AuthorId { get; set; }
  public required string Content { get; set; }
  public required DateTime CreatedAt { get; set; }
}
