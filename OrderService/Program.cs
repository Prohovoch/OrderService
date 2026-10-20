using DotNetEnv;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;
using Serilog;
using System.Text.Json.Serialization;

namespace OrderService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Env.Load();
            // var teletgamBotToken = Env.GetString("TELEGRAM_BOT_TOKEN");
            // when i implement env vars.
            // Add services to the container.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                // .WriteTo.File("logs/my-logs.txt", rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Starting web app");
                var builder = WebApplication.CreateBuilder(args);
                builder.Services.AddMemoryCache();
                builder.Services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
                builder.Services.AddTransient<OrderService.src.Security.Helpers.OtpGenerate>();


                builder.Services.AddFastEndpoints();
                builder.Services.OpenApiDocument(o => 
                {
                    o.AddAuth("ApiKey", new()
                    {
                        Name = Env.GetString("BOTAPIHEADER"),
                        In =  Microsoft.OpenApi.ParameterLocation.Header,
                        Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey
                        
                    });
                    o.DocumentName = "service";
                    o.Version = "v1";
                    o.Title = "Order Service API";
                });
                
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
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await db.Database.MigrateAsync();
                }
                

                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.UseSwaggerUI(options => {
                        options.SwaggerEndpoint("/openapi/service.json", "OrderService Api V1");
                    });

                }
              

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
