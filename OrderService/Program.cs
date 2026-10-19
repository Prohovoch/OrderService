using FastEndpoints;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Serilog;
using DotNetEnv;
using Microsoft.AspNetCore.Http.Timeouts;
namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            // var teletgamBotToken = Env.GetString("TELEGRAM_BOT_TOKEN");
            // when i implement env vars.
            // Add services to the container.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/my-logs.txt", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web app");
                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddMemoryCache();
                builder.Services.AddFastEndpoints();
                builder.Services.AddRequestTimeouts(options =>
                {
                    options.DefaultPolicy = new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromSeconds(5), // as an example.
                        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                        WriteTimeoutResponse = async (HttpContext context) =>
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new { error = "Default time request timed out." });
                        }
                    };
                    options.AddPolicy("Medium", new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromSeconds(10), // as an example.
                        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                        WriteTimeoutResponse = async (HttpContext context) =>
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new { error = "Medium request time (10s) out." });
                        }
                    });
                    options.AddPolicy("Long", new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromSeconds(15), // as an example.
                        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout,
                        WriteTimeoutResponse = async (HttpContext context) =>
                        {
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsJsonAsync(new { error = "Long request time (15s) out." });
                        }
                    });
                });
               
                var app = builder.Build();
                // TODO: tg client or http factory client impl here.


                app.UseRouting();
                app.UseRequestTimeouts();

                app.UseDefaultExceptionHandler(useProblemDetails: true)
                    .UseFastEndpoints(c => c.Serializer.Options.Converters.Add(new JsonStringEnumConverter()));
                // Configure the HTTP request pipeline.
                 
                app.UseHttpsRedirection();
                
              

              

                app.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "App shut down without nornal condition. Exception occurred {Exception}.", ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }
           
        }
    }
}
