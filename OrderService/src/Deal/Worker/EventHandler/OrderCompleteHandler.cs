using FastEndpoints;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace OrderService.src.Deal.Worker.EventHandler
{
    public class OrderCompleteHandler(ITelegramBotClient botClient,  ILogger<OrderCompleteHandler> Logger) : IEventHandler<OrderCompletedEventObj>
    {
       // TODO: complete this. Send an message through Telegram.Bot to user. mock this with potential integrational test.
        
        public async Task HandleAsync(OrderCompletedEventObj eventObj, CancellationToken ct)
        {
            // Implement the logic to handle the order completion event.
            // For example, send a notification to the user via Telegram.Bot.
            try
            {
                Logger.LogInformation($"Sending order completion message to user {eventObj.TelegramId} for order {eventObj.OrderNumber}.");
                // http client lol.
                await botClient.SendMessage

                    (
                    chatId: eventObj.TelegramId,
                    text: $"Your order with number {eventObj.OrderNumber} has been completed successfully. Come to payment station.",
                    cancellationToken: ct


                    );
            }

            catch (ApiRequestException ex) when (ex.ErrorCode == 403)
            {
                Logger.LogError("Failed to send message to user {TelegramId}: Bot was blocked.", eventObj.TelegramId);
                // Handle the case where the bot is blocked by the user.
               
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while sending message to user {TelegramId}.", eventObj.TelegramId);
                // Handle other exceptions that may occur during message sending.
               
            }
            finally
            {
                Logger.LogInformation($"Finished handling order completion event for user {eventObj.TelegramId}.");
            }
        }

    }
}
// all of this will be rewritten with proper logs a little bit later.