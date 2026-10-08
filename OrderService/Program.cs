using FastEndpoints;
using System.Text.Json.Serialization;
using Telegram.Bot;
using Serilog;
namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
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
                builder.Services.AddFastEndpoints();
                builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("YOUR_TELEGRAM_BOT_TOKEN")); //mock. gonna change it 
                var app = builder.Build();
                // TODO: tg client or http factory client impl here.

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
