using DotNetEnv;
using System.Security.Cryptography;
using System.Text;

namespace OrderService.src.Security.Middleware
{
    public class BotApiKeyMiddleware(RequestDelegate next)
    {
       
        private readonly string _expectedKey = Env.GetString("TGBOT_API") ?? string.Empty;
        private const string HeaderName = "X-Bot-Api-Key";

        public async Task InvokeAsync(HttpContext context)
        {
            if (string.IsNullOrEmpty(_expectedKey))
            {
                
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server missconfiguration. Tg Bot api has not been set");
                return;
            }
            if (!context.Request.Headers.TryGetValue(HeaderName, out var providedResult) || string.IsNullOrEmpty(providedResult))
            {
                await SendUnauthorizedResponse(context);
                return;
            }

            var provided = providedResult.ToString();
            var expectedBytes = Encoding.UTF8.GetBytes(_expectedKey);
            var providedBytes = Encoding.UTF8.GetBytes(provided);

            if (!CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
            {
                await SendUnauthorizedResponse(context);
                return;
            }

            await next(context);
        }
        private static async Task SendUnauthorizedResponse(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or missing bot API key." });
        }
    }
}
