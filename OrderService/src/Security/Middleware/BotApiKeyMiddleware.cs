using DotNetEnv;
using System.Security.Cryptography;
using System.Text;

namespace OrderService.src.Security.Middleware
{

/// <summary>
/// Middleware to validate the bot API key from the request headers.
/// </summary>
    public class BotApiKeyMiddleware(RequestDelegate next)
    {
        // This param is used to store the expected API key from environment variables.
        private readonly string _expectedKey = Env.GetString("TGBOT_API") ?? string.Empty;
        // This param is used to store the expected  header name from environment variables.
        private readonly string _headerName = Env.GetString("BOTAPIHEADER") ?? string.Empty;
        /// <summary>
        /// This method is invoked for each HTTP request to validate the bot API key.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {

            if(string.IsNullOrEmpty(_headerName))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid bot API header name.");
                return;
            }
            if (string.IsNullOrEmpty(_expectedKey))
            {
                
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Server missconfiguration. Invalid bot API key.");
                return;
            }
            if (!context.Request.Headers.TryGetValue(_headerName, out var providedResult) || string.IsNullOrEmpty(providedResult))
            {
                await SendUnauthorizedResponse(context);
                return;
            }
            // Use a constant-time comparison to prevent timing attacks.
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
        /// <summary>
        /// Sends a 401 Unauthorized response with a JSON error message when the bot API key is invalid or missing.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>An Error from Task</returns>
        private static async Task SendUnauthorizedResponse(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or missing bot API key." });
        }
    }
}
