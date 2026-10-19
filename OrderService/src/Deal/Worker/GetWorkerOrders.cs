using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Deal.Customer;



namespace OrderService.src.Deal.Worker
{
    public class GetWorkerOrders(ApplicationDbContext dbContext) : Endpoint<GetWorkerOrdersRequest, GetWorkerOrdersResponse>
    {

        public override void Configure()
        {
            Get("api/admin/orders/picked");
            AllowAnonymous();
            Validator<GetWorkerOrdersValidator>();
        }

        public override async Task HandleAsync(GetWorkerOrdersRequest req, CancellationToken ct)
        {


            var entityId = await dbContext.Workers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                Logger.LogWarning("Unauthorized access attempt by TelegramId: {TelegramId}", req.TelegramId);
                await Send.ForbiddenAsync();
                return;
            }

            var workerOrders = (await dbContext.Orders
                .Where(o => o.WorkerId == entityId.Value).AsNoTracking().Select(x => new
                {
                    x.Id,
                    x.DisplayOrderNumber,
                    x.ClientName,
                    x.ClientSurname,
                    x.WorkerName, // fact: workerName! - because he is in system. if not. he couldnt do that. In this case, i just fighting with null vals. help me :___(
                    x.WorkerSurname,
                    x.CustomerPhoneNumber,
                    x.Status,
                    x.CreatedAt,
                    Items = x.Items.Select(oi => new GetOrderResponseItems
                    {

                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                }).ToListAsync(ct))
                .Select(x => new WorkerOrdersResponse
                {
                    OrderId = x.Id,
                    OrderNumber = x.DisplayOrderNumber,
                    ClientName = x.ClientName,
                    ClientSurname = x.ClientSurname,
                    WorkerName = x.WorkerName ?? "No data",
                    WorkerSurname = x.WorkerSurname ?? "No data",
                    ClientPhNumber = x.CustomerPhoneNumber ?? "No data",
                    Status = x.Status switch
                    {
                        OrderStatus.Processing => OrderStatusResponse.Processing,
                        OrderStatus.Cooked => OrderStatusResponse.Cooked,
                        OrderStatus.Stopped => OrderStatusResponse.Stopped,
                        OrderStatus.Closed => OrderStatusResponse.Closed,
                        _ => throw new InvalidOperationException($"Inconsistent data state: Order '{x.DisplayOrderNumber}' (ID: {x.Id}) has an unsupported status {x.Status}.")
                    },
                    CreatedAt = x.CreatedAt , // TODO: Replace with actual creation time if available
                    Items = x.Items
                }).ToList();
            Logger.LogInformation("Retrieved {Count} orders for worker with TelegramId: {TelegramId}", workerOrders.Count, req.TelegramId);
            await Send.OkAsync(new GetWorkerOrdersResponse { WorkerOrders = workerOrders });


        }
    }
           
         

public class GetWorkerOrdersValidator : Validator<GetWorkerOrdersRequest>
    {
        public GetWorkerOrdersValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
        }
    }
    public sealed record GetWorkerOrdersRequest
    {
        [FromHeader("Worker-Telegram-Id")]
        public long TelegramId { get; init; }

    }

    public sealed record GetOrderResponseItems
    {
        public string ProductName { get; init; } = null!;
        public int Quantity { get; init; }
        public decimal TotalPrice { get; init; } // ?
        // price at purchase.
        public decimal Price { get; init; }
    }
    public sealed record GetWorkerOrdersResponse
    {

        public List<WorkerOrdersResponse> WorkerOrders { get; init; } = [];

    }
    public enum OrderStatusResponse
    {
        
        Processing,
        Cooked,
        Stopped,
        Closed
    }
    public sealed record WorkerOrdersResponse
    {
        public Guid OrderId { get; init; }
        public string WorkerName { get; init; } = null!;
        public string WorkerSurname { get; init; } = null!;
        public required string OrderNumber { get; init; }
        public required string ClientName { get; init; }
        public required string ClientSurname { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public string ClientPhNumber { get; init; } = null!;
        public List<GetOrderResponseItems> Items { get; init; } = [];
        public OrderStatusResponse Status { get; init; }
    }


}




