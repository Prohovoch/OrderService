using FastEndpoints;
using Telegram.Bot;
namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddFastEndpoints();
            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient("YOUR_TELEGRAM_BOT_TOKEN")); //mock. gonna change it 
            // when i implement env vars.
            // Add services to the container.


            var app = builder.Build();
            // TODO: tg client or http factory client impl here.

            // Configure the HTTP request pipeline.
            app.UseFastEndpoints();
            app.UseHttpsRedirection();

            // middleware;

            // app.MapControllers();

            app.Run();
        }
    }
}
