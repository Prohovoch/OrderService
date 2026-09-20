using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Deal.Customer;



namespace OrderService.src.Deal.Worker
{
    public class GetOrders(ApplicationDbContext dbContext) : Endpoint<GetOrdersRequest, GetOrdersResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/worker/{telegramId}/orders");
            AllowAnonymous();
            Validator<LookCompleteOrdersValidator>();
        }

        public override async Task HandleAsync(GetOrdersRequest req, CancellationToken ct)
        {


            var entityId = await _dbContext.Admins.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                await Send.ForbiddenAsync();
                return;
            }

            var createdOrders = await _dbContext.Orders
                .Where(o => o.Status == OrderStatus.Closed).AsNoTracking().Select(x => new LookCompleteOrdersResponse
                {
                    OrderId = x.Id,
                    OrderNumber = x.DisplayOrderNumber,
                    ClientName = x.ClientName,
                    ClientSurname = x.ClientSurname,
                    WorkerName = x.WorkerName ?? "No Data", // fact: workerName! - because he is in system. if not. he couldnt do that. In this case, i just fighting with null vals. help me :___(
                    WorkerSurname = x.WorkerSurname ?? "No Data",
                    ClientPhNumber = x.CustomerPhoneNumber ?? "Unknown",
                    CreatedAt = x.CreatedAt,

                    Items = x.Items.Select(oi => new GetOrderResponseItems
                    {

                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                })
                .ToListAsync(ct);

            await Send.OkAsync(new GetOrdersResponse { Orders = createdOrders });


        }
    }

    public class LookCompleteOrdersValidator : Validator<GetOrdersRequest>
    {
        public LookCompleteOrdersValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
        }
    }
    public sealed record GetOrdersRequest
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
    public sealed record GetOrdersResponse
    {

        public List<LookCompleteOrdersResponse> Orders { get; init; } = [];

    }
    public sealed record LookCompleteOrdersResponse
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
        public OrderStatus Status { get; init; }
    }


}




