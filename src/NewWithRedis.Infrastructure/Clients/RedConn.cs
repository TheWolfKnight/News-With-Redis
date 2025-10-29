using StackExchange.Redis;
using System.Text.Json;
using NewsWithRedis.Infrastructure.Helpers;

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

    public async Task<T>? GetHash<T>(string key) //where T : new()
    {
        var entries = await this.db.HashGetAllAsync(key);
        if (entries.Length == 0) { return default; }

        return HashHelper.FromHashEntries<T>(entries);
    }

    public async Task<List<T>> GetHashSet<T>(string key) //where T : new()
    {
        string indexKey = $"{key}:comments";

        var memberKeys = await this.db.SortedSetRangeByRankAsync(indexKey, 0, -1, Order.Ascending);
        if (memberKeys.Length == 0) { return default; }

        var tasks = new List<Task<HashEntry[]>>(memberKeys.Length);
        foreach (var member in memberKeys)
        {
            RedisKey mKey = (string)member;
            tasks.Add(this.db.HashGetAllAsync(mKey));
        }
        await Task.WhenAll(tasks);

        var result = new List<T>(tasks.Count);
        foreach (var t in tasks)
        {
            var entries = t.Result;
            if (entries.Length > 0)
            {
                var obj = HashHelper.FromHashEntries<T>(entries);
                result.Add(obj);
            }
        }
        return result;
    }

    public async Task SetHash<T>(string key, T Payload, TimeSpan ttl)
    {
        var entries = HashHelper.ToHashEntries(Payload);
        await this.db.HashSetAsync(key, entries);
        await this.db.KeyExpireAsync(key, ttl);
    }

    public async Task SetHashAndSet<T>(string key, List<T> Payload, TimeSpan ttl)
    {
        var build = HashHelper.BuildHashSet(Payload, key);
        var transaction = this.db.CreateTransaction();

        foreach (var m in build.Members)
        {
            _ = transaction.HashSetAsync(m.key, m.Entries);
            _ = transaction.KeyExpireAsync(m.key, ttl);
        }

        foreach (var entry in build.IndexEntries)
        {
            _ = transaction.SortedSetAddAsync(build.IndexKey, entry.memberkey, entry.score);
        }

        _ = transaction.KeyExpireAsync(build.IndexKey, ttl);

        await transaction.ExecuteAsync();
    }

    public async Task SetHashParentChildren<T, Tk>(string key, T Parent, List<Tk> Children, TimeSpan ttl)
    {
        var transaction = this.db.CreateTransaction();
        EnqueueSetHash(transaction, key, Parent, ttl);
        EnqueueSetHashAndSet(transaction, key, Children, ttl);
        await transaction.ExecuteAsync();
    }

    public async Task<bool> FlushAllAsync()
    {
        var cmd = this.db.ExecuteAsync("FLUSHDB", "ASYNC");
        RedisResult result = await cmd;
        return result.ToString() == "OK" ? true : false;
    }

    private void EnqueueSetHash<T>(ITransaction transaction, string key, T Payload, TimeSpan ttl)
    {
        _ = transaction.HashSetAsync(key, HashHelper.ToHashEntries(Payload));
        _ = transaction.KeyExpireAsync(key, ttl);
    }

    private void EnqueueSetHashAndSet<T>(ITransaction transaction, string key, List<T> Payload, TimeSpan ttl)
    {
        if (Payload == null || Payload.Count == 0) { return; }
        var build = HashHelper.BuildHashSet(Payload, key);

        foreach (var m in build.Members)
        {
            _ = transaction.HashSetAsync(m.key, m.Entries);
            _ = transaction.KeyExpireAsync(m.key, ttl);
        }

        foreach (var entry in build.IndexEntries)
        {
            _ = transaction.SortedSetAddAsync(build.IndexKey, entry.memberkey, entry.score);
        }

        _ = transaction.KeyExpireAsync(build.IndexKey, ttl);
    }
}
