using FastEndpoints;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace OrderService.src.Deal.Worker.EventHandler
{
    public class OrderCompleteHandler(ITelegramBotClient botClient) : IEventHandler<OrderCompletedEventObj>
    {
       // TODO: complete this. Send an message through Telegram.Bot to user. mock this with potential integrational test.
        
        public async Task HandleAsync(OrderCompletedEventObj eventObj, CancellationToken ct)
        {
            // Implement the logic to handle the order completion event.
            // For example, send a notification to the user via Telegram.Bot.
            try
            {
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
                // Handle the case where the bot is blocked by the user.
                Console.WriteLine($"Failed to send message to user {eventObj.TelegramId}: Bot was blocked.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions that may occur during message sending.
                Console.WriteLine($"An error occurred while sending message to user {eventObj.TelegramId}: {ex.Message}");
            } 
        }

    }
}
// all of this will be rewritten with proper logs a little bit later.