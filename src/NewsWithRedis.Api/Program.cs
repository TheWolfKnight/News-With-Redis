using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using NewsWithRedis.Common.DTOs;
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

            //-------------- With Cache Aside ------------------//

            app.MapGet("/api/user/{id}", async (int id, RedConn cache, SqlClient repository) =>
            {
                var key = $"user:{id}";

                var cached = cache.GetJson<User>(key);
                if (cached is not null) { return Results.Ok(Response.From(cached, "Redis")); }

                var user = await repository.GetAsync<User>(id);
                if (user is null) { return Results.NotFound(); }

                cache.SetJson(key, user, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(user, "SQL"));
            });

            app.MapGet("/api/user", async (RedConn cache, SqlClient repository) =>
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

            /*app.MapGet("/api/articles/{id}", async (int id, RedConn cache, SqlClient repository) =>
            {
                var key = $"article:{id}"; //Redis Key formating

                var cached = cache.GetJson<Article>(key);
                if (cached is not null) { return Results.Ok(Response.From(cached, "Redis")); }

                var article = await repository.GetAsync<Article>(id);
                if (article is null) { return Results.NotFound(); }

                cache.SetJson(key, article, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(article, "SQL"));
            });*/

            app.MapGet("/api/article/{id}", async (int id, RedConn cache, SqlClient respository) => //with-comments
            {
                var key = $"article:{id}:with-comments";

                var cached = cache.GetJson<WithChildrenDTO<Article, Comment>>(key);
                if (cached is not null) { return Results.Ok(Response.From(cached, "Redis")); }

                var result = await respository.GetParentChildrenAsync<Article, Comment>(id);
                if (result is null) { return Results.NotFound(); }

                cache.SetJson(key, result, TimeSpan.FromSeconds(15));
                return Results.Ok(Response.From(result, "SQL"));
            });

            app.MapGet("/api/article", async (RedConn cache, SqlClient repository) =>
            {
                var key = "article:all"; //this is cringe if DataSet grows

                var cached = cache.GetJson<List<Article>>(key);
                if (!cached.IsNullOrEmpty()) { return Results.Ok(Response.From(cached, "Redis")); }

                var articles = (await repository.GetAllAsync<Article>()).ToList();
                if (articles.IsNullOrEmpty()) { return Results.NotFound();}

                cache.SetJson(key, articles, TimeSpan.FromMinutes(15));
                return Results.Ok(Response.From(articles, "SQL"));
            });

            app.MapDelete("/api/cache", async (RedConn cache) =>
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
