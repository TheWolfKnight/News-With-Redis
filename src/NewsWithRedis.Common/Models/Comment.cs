using System;

namespace NewsWithRedis.Common.Models
{
    public class Comment
    {
        public required int ArticleId { get; set; }
        public required int UserId { get; set; }
        public required string Content { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
