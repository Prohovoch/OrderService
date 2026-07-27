using FastEndpoints;
namespace OrderService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddFastEndpoints();

            // Add services to the container.


            var app = builder.Build();


            // Configure the HTTP request pipeline.
            app.UseFastEndpoints();
            app.UseHttpsRedirection();

            // middleware;

            // app.MapControllers();

            app.Run();
        }
    }
}
