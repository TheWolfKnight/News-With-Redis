
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using NewsWithRedis.Common.Models;
using NewsWithRedis.Infrastructure.Clients.SQL;
using System.Data.SqlTypes;

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
            {
                var cString = builder.Configuration["cStrings.SQl.DefaultConnection"];
                return new SqlClient(cString!);
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

            app.Run();
        }
    }
}
