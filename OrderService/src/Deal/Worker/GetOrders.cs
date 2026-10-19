using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Deal.Worker
{
    public class GetOrders(ApplicationDbContext dbContext) : Endpoint<GetOrdersRequest, GetOrdersResponse>
    {

        public override void Configure()
        {
            Get("api/worker/orders/pool");
            AllowAnonymous();
            Validator<GetOrdersValidator>();
        }

        public override async Task HandleAsync(GetOrdersRequest req, CancellationToken ct)
        {


            bool entityExists = await dbContext.Admins.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (!entityExists)
            {
                Logger.LogWarning("Entity with TelegramId {TelegramId} not found in Admins table.", req.TelegramId);
                await Send.ForbiddenAsync();
                return;
            }

            var createdOrders = (await dbContext.Orders
                .Where(o => o.Status == OrderStatus.Created).AsNoTracking().Select(x => new
                {
                    x.Id,
                    x.DisplayOrderNumber,
                    x.ClientName,
                    x.ClientSurname,
                    x.CustomerPhoneNumber,
                    x.CreatedAt,
                    Items = x.Items.Select(oi => new GetOrderItems
                    {

                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                }).ToListAsync(ct))
                .Select(x => new LookOrdersResponse
                {
                    OrderId = x.Id,
                    OrderNumber = x.DisplayOrderNumber,
                    ClientName = x.ClientName,
                    ClientSurname = x.ClientSurname,
                    ClientPhNumber = x.CustomerPhoneNumber ?? "No data",
                   
                    CreatedAt = x.CreatedAt, // TODO: Replace with actual creation time if available
                    Items = x.Items
                }).ToList();
            
            
            Logger.LogInformation("Retrieved {Count} created orders for TelegramId {TelegramId}.", createdOrders.Count, req.TelegramId);
            await Send.OkAsync(new GetOrdersResponse { OrdersDto = createdOrders });


        }
    }

    public class GetOrdersValidator : Validator<GetOrdersRequest>
    {
        public GetOrdersValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
        }
    }
    public sealed record GetOrdersRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }

    }

    public sealed record GetOrderItems
    {
        public string ProductName { get; init; } = null!;
        public int Quantity { get; init; }
        public decimal TotalPrice { get; init; } // ?
        // price at purchase.
        public decimal Price { get; init; }
    }
    public sealed record GetOrdersResponse
    {

        public List<LookOrdersResponse> OrdersDto { get; init; } = [];

    }
    public sealed record LookOrdersResponse
    {
        public Guid OrderId { get; init; }
        public required string OrderNumber { get; init; }
        public required string ClientName { get; init; }
        public required string ClientSurname { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public string ClientPhNumber { get; init; } = null!;
        public List<GetOrderItems> Items { get; init; } = [];
        
    }


}




