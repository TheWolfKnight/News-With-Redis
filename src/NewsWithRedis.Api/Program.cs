using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using NewsWithRedis.Common.Models;
using NewsWithRedis.Infrastructure.Clients.RED;
using NewsWithRedis.Infrastructure.Clients.SQL;

namespace NewsWithRedis.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<SqlClient>(x =>
            { // Could replace Json here for more speed
                var cString = builder.Configuration["cStrings:SQl:DefaultConnection"];
                return new SqlClient(cString!);
            });
            builder.Services.AddSingleton<ConnectionMultiplexer>(y => {
                var cString = builder.Configuration["cStrings:RED:DefaultConnection"];
                return ConnectionMultiplexer.Connect(cString!);
            });
            builder.Services.AddScoped<RedConn>(y =>
            {
                return new RedConn(y.GetRequiredService<ConnectionMultiplexer>());
            });


            // https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();

            //------------- Straight Endpoints ------------------//

            app.MapGet("/Api/Users/{id}", async (int id, SqlClient repository) =>
            {
                var user = await repository.GetAsync<User>(id);
                return user is null ? Results.NotFound() : Results.Ok(user);
            });

            app.MapGet("/Api/Users/All", async (SqlClient repository) =>
            {
                var users = await repository.GetAllAsync<User>();
                return users.IsNullOrEmpty() ? Results.NotFound() : Results.Ok(users);
            });

            app.MapGet("/Api/Articles/{id}", async (int id, SqlClient repository) =>
            {
                var article = await repository.GetAsync<Article>(id);
                return article is null ? Results.NotFound() : Results.Ok(article);
            });

            app.MapGet("/Api/Articles/ALL", async (SqlClient repository) =>
            {
                var articles = await repository.GetAllAsync<Article>();
                return articles.IsNullOrEmpty() ? Results.NotFound() : Results.Ok(articles);
            });

            //-------------- With Cache Aside ------------------//

            app.MapGet("/Api/Cache/Users/{id}", async (int id, RedConn cache, SqlClient repository) =>
            {
                var key = $"user:{id}";

                var cached = cache.GetJson<User>(key);
                if (cached is not null) { return Results.Ok(Response.From(cached, "Redis")); }

                var user = await repository.GetAsync<User>(id);
                if (user is null) { return Results.NotFound(); }

                cache.SetJson(key, user, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(user, "SQL"));
            });

            app.MapGet("/Api/Cache/Users/ALL", async (RedConn cache, SqlClient repository) =>
            {
                var key = $"user:all";

                var cached = cache.GetJson<List<User>>(key);
                if (!cached.IsNullOrEmpty()) { Console.WriteLine("Fetched Users/All Cache"); return Results.Ok(Response.From(cached, "Redis")); }

                var users = (await repository.GetAllAsync<User>()).ToList();
                if (users.IsNullOrEmpty()) { Console.WriteLine("Couldnt find anything at Users/All"); return Results.NotFound(); }

                cache.SetJson(key, users, TimeSpan.FromMinutes(15));
                Console.WriteLine("Updated Cache at Users/All");
                return Results.Ok(Response.From(users, "SQL"));
            });

            app.MapGet("/Api/Cache/Articles/{id}", async (int id, RedConn cache, SqlClient repository) =>
            {
                var key = $"article:{id}"; //Redis Key formating

                var cached = cache.GetJson<Article>(key);
                if (cached is not null) { return Results.Ok(Response.From(cached, "Redis")); }

                var article = await repository.GetAsync<Article>(id);
                if (article is null) { return Results.NotFound(); }

                cache.SetJson(key, article, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(article, "SQL"));
            });

            app.MapGet("/Api/Cache/Articles/ALL", async (RedConn cache, SqlClient repository) =>
            {
                var key = "article:all"; //this is cringe if DataSet grows

                var cached = cache.GetJson<List<Article>>(key);
                if (!cached.IsNullOrEmpty()) { return Results.Ok(Response.From(cached, "Redis")); }

                var articles = (await repository.GetAllAsync<Article>()).ToList();
                if (articles.IsNullOrEmpty()) { return Results.NotFound();}

                cache.SetJson(key, articles, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(articles, "SQL"));
            });

            app.MapGet("/Api/Cache/Reset", async (RedConn cache) =>
            {
                var result = await cache.FlushAllAsync();
                return result is true 
                    ? Results.Ok("Flushed OK!")
                    : Results.Problem("Something went wrong while flushing");
            });

            app.Run();
        }

        public static class Response
        {
            public static ResponseRecord<T> From<T>(T data, string source) => new(data, source);
        }
    }

    public record ResponseRecord<T>(T Data, string Source);
}
