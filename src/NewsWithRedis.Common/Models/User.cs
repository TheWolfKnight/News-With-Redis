using System;

namespace NewsWithRedis.Common.Models;

public class User
{
  public required string Username { get; set; }
  public required string Email { get; set; }
  public required DateTime CreatedAt { get; set; }
}
