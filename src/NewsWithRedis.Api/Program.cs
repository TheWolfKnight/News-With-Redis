
using Microsoft.AspNetCore.DataProtection.Repositories;
using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using NewsWithRedis.Common.Models;
using NewsWithRedis.Infrastructure.Clients.SQL;
using NewsWithRedis.Infrastructure.Clients.RED;
using Microsoft.AspNetCore.Http.HttpResults;

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
                var cString = builder.Configuration["cStrings:RED.DefaultConnection"];
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


            /*app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast");*/

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
                var article = await repository.GetAsync<Artical>(id);
                return article is null ? Results.NotFound() : Results.Ok(article);
            });

            app.MapGet("/Api/Articles/ALL", async (SqlClient repository) =>
            {
                var articles = await repository.GetAllAsync<Artical>();
                return articles.IsNullOrEmpty() ? Results.NotFound() : Results.Ok(articles);
            });

            app.MapGet("/Api/Cache/Articles/{id}", async (int id, RedConn cache, SqlClient repository) =>
            {
                var key = $"user:{id}"; //Redis Key formating

                var cached = cache.GetJson<Artical>(key);
                if (cached is not null) { return Results.Ok((data: cached, source: "Redis")); }

                var article = await repository.GetAsync<Artical>(id);
                if (article is null) { return Results.NotFound(); };

                cache.SetJson(key, article, TimeSpan.FromMinutes(15));
                return Results.Ok((data: article, source: "SQL"));
            });

            app.MapGet("/Api/Cache/Articles/ALL", async (RedConn cache, SqlClient repository) =>
            {
                var key = "user:all"; //this is cringe of DataSet grows

                var cached = cache.GetJson<List<Artical>>(key);
                if (!cached.IsNullOrEmpty()) { return Results.Ok((data: cached, Source: "Redis")); }

                var articles = (await repository.GetAllAsync<Artical>()).ToList();
                if (articles.IsNullOrEmpty()) { return Results.NotFound();}

                cache.SetJson(key, articles, TimeSpan.FromMinutes(15));
                return Results.Ok((data: articles, source: "SQL"));
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
    }
}
