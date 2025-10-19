using StackExchange.Redis;
using System.Text.Json;

namespace NewsWithRedis.Infrastructure.Clients.RED;

public sealed class RedConn
{
    private readonly IDatabase db; //what Stackexchange uses
    private static readonly JsonSerializerOptions json = new();

    public RedConn(ConnectionMultiplexer conn)
    {
        this.db = conn.GetDatabase();
    }

    public RedisValue? Get(string key)
    {
        var value = this.db.StringGet(key);
        return value.HasValue ? value : string.Empty;
    }

    public void Set(string key, string value, TimeSpan ttl)
    {
        this.db.StringSet(key, value, ttl);
    }

    public T? GetJson<T>(string key)
    {
        RedisValue value = this.db.StringGet(key);
        if (!value.HasValue) { return default; }

        return JsonSerializer.Deserialize<T>(value, RedConn.json);
    }

    public void SetJson<T>(string key, T payload, TimeSpan ttl)
    {
        var value = JsonSerializer.Serialize(payload, RedConn.json);
        this.db.StringSet(key, value, ttl);
    }

    public async Task<bool> FlushAllAsync()
    {
        var cmd = this.db.ExecuteAsync("FLUSHDB", "ASYNC");
        RedisResult result = await cmd;
        return result.ToString() == "OK" ? true : false;
    }
}
