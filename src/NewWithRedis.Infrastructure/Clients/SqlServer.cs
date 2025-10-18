using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using NewsWithRedis.Common.DTOs;

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
        string table = typeof(T).Name; //In the future, use Enums here
        string sql = $"SELECT * FROM [{table}] WHERE id = {id}";

        using var conn = CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<T>(sql);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>()
    {
        string table = typeof(T).Name;
        string sql = $"SELECT * FROM [{table}]";

        using var conn = CreateConnection();
        return await conn.QueryAsync<T>(sql);
    }

    public async Task<WithChildrenDTO<TParent, TChild>?> 
        GetParentChildrenAsync<TParent, TChild>(int id)
    {
        string parentTable = typeof(TParent).Name;
        string childTable  = typeof(TChild).Name;
        string fk          = $"{parentTable}Id";
        
        string sql = $@"
            SELECT * FROM [{parentTable}] WHERE Id = {id};
            SELECT * from [{childTable}] WHERE  [{fk}] = {id};";

        using var conn = CreateConnection();
        using var multi = await conn.QueryMultipleAsync(sql);

        var parent = await multi.ReadSingleOrDefaultAsync<TParent>();
        if (parent is null) { return null; }
        var children = (await multi.ReadAsync<TChild>()).ToList();
        return new WithChildrenDTO<TParent, TChild> ( parent, children );
    }
}