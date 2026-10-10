using FastEndpoints;
using OrderService.src.Deal.Worker;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace OrderService.src.Auth.Worker.EventHandler
{
    public class OtpGeneratedHandler(ITelegramBotClient botClient, ILogger<OtpGeneratedHandler> Logger) : IEventHandler<CodeGeneratedEventObj>
    {
        // TODO: complete this. Send an message through Telegram.Bot to user. mock this with potential integrational test.

        public async Task HandleAsync(CodeGeneratedEventObj eventObj, CancellationToken ct)
        {
            // Implement the logic to handle the order completion event.
            // For example, send a notification to the user via Telegram.Bot.
            try
            {
                Logger.LogInformation($"Sending order completion message to user {eventObj.telegramId},  code : {eventObj.code}.");
                // http client lol.
                await botClient.SendMessage

                    (
                    chatId: eventObj.telegramId,
                    text: $"Code {eventObj.code} has been generated successfully. Enter it before expiration.",
                    cancellationToken: ct


                    );
            }

            catch (ApiRequestException ex) when (ex.ErrorCode == 403)
            {
                Logger.LogError("Failed to send message to user {TelegramId}: Bot was blocked.", eventObj.telegramId);
                // Handle the case where the bot is blocked by the user.
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while sending message to user {TelegramId}.", eventObj.telegramId);
                // Handle other exceptions that may occur during message sending.
               
            }
            finally
            {
                Logger.LogInformation($"Finished handling order completion event for user {eventObj.telegramId}.");
            }
        }

    }
}

