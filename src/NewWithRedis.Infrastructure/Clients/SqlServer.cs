using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace NewsWithRedis.Infrastructure.Clients.SQL;

public sealed class SqlClient
{
    private readonly string cString;
    private IDbConnection CreateConnection() => new SqlConnection(this.cString);

    public SqlClient(string connectionString)
    {
        this.cString = connectionString;
    }

    public async Task<T> GetAsync<T>(int id)
    {
        var table = typeof(T).Name;
        var sql = $"SELECT * FROM [{table}] WHERE id = {id}";

        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(sql);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>()
    {
        var table = typeof(T).Name;
        var sql = $"SELECT * FROM [{table}]";

        using var conn = CreateConnection();
        return await conn.QueryAsync<T>(sql);
    }
}