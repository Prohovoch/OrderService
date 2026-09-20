using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Deal.Customer;



namespace OrderService.src.Deal.Worker
{
    public class GetWorkerOrders(ApplicationDbContext dbContext) : Endpoint<GetWorkerOrdersRequest, GetWorkerOrdersResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/admin/{telegramId}/orders/picked");
            AllowAnonymous();
            Validator<GetWorkerOrdersValidator>();
        }

        public override async Task HandleAsync(GetWorkerOrdersRequest req, CancellationToken ct)
        {


            var entityId = await _dbContext.Admins.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                await Send.ForbiddenAsync();
                return;
            }

            var workerOrders = await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.Processing && o.WorkerId == entityId.Value).AsNoTracking().Select(x => new WorkerOrdersResponse
                {
                    OrderId = x.Id,
                    OrderNumber = x.DisplayOrderNumber,
                    ClientName = x.ClientName,
                    ClientSurname = x.ClientSurname,
                    WorkerName = x.WorkerName ?? "No Data", // fact: workerName! - because he is in system. if not. he couldnt do that. In this case, i just fighting with null vals. help me :___(
                    WorkerSurname = x.WorkerSurname ?? "No Data",
                    ClientPhNumber = x.CustomerPhoneNumber ?? "Unknown",
               
                    Items = x.Items.Select(oi => new GetOrderResponseItems
                    {

                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                })
                .ToListAsync(ct);

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
        [BindFrom("telegramId")]
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
    public sealed record WorkerOrdersResponse
    {
        public Guid OrderId { get; init; }
        public string WorkerName { get; init; } = null!;
        public string WorkerSurname { get; init; } = null!;
        public required string OrderNumber { get; init; }
        public required string ClientName { get; init; }
        public required string ClientSurname { get; init; }
        public DateTimeOffset CompletedAt { get; init; }
        public string ClientPhNumber { get; init; } = null!;
        public List<GetOrderResponseItems> Items { get; init; } = [];
        public OrderStatus Status { get; init; }
    }


}




