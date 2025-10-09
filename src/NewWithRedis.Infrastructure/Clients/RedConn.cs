using StackExchange.Redis;
using System;

namespace RedDash.Utilities.RedConn;

public class RedConn : IDisposable
{
    private readonly ConnectionMultiplexer conn;
    private readonly IDatabase db;

    public RedConn(string connectionString = "localhost:6379")
    {
        conn = ConnectionMultiplexer.Connect(connectionString);
        db = conn.GetDatabase();
    }

    public RedisValue? Get(string key)
    {
        var value = this.db.StringGet(key);
        return value.HasValue ? value : string.Empty;
    }

    public void Set(string key, string value)
    {
        this.db.StringSet(key, value);
    }

    public void Dispose()
    {
        this.conn.Close();
        this.conn.Dispose();
    }
}