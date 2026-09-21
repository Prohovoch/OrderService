using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Deal.Worker
{
    public class GetOrders(ApplicationDbContext dbContext) : Endpoint<GetOrdersRequest, GetOrdersResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/worker/orders/pool");
            AllowAnonymous();
            Validator<GetOrdersValidator>();
        }

        public override async Task HandleAsync(GetOrdersRequest req, CancellationToken ct)
        {


            bool entityExists = await _dbContext.Admins.AnyAsync(x => x.TgId == req.TelegramId, ct);
            if (!entityExists)
            {
                await Send.ForbiddenAsync();
                return;
            }

            var createdOrders = await _dbContext.Orders.Where(x => x.Status == OrderStatus.Created)
                .AsNoTracking().Select(x => new LookOrdersResponse
                {
                    OrderId = x.Id,
                    OrderNumber = x.DisplayOrderNumber,
                    ClientName = x.ClientName,
                    ClientSurname = x.ClientSurname,
                    WorkerName = x.WorkerName ?? "No Data", // fact: workerName! - because he is in system. if not. he couldnt do that. In this case, i just fighting with null vals. help me :___(
                    WorkerSurname = x.WorkerSurname ?? "No Data",
                    ClientPhNumber = x.CustomerPhoneNumber ?? "Unknown",
                    CreatedAt = x.CreatedAt,

                    Items = x.Items.Select(oi => new GetOrderItems
                    {

                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                })
                .ToListAsync(ct);

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
        public string WorkerName { get; init; } = null!;
        public string WorkerSurname { get; init; } = null!;
        public required string OrderNumber { get; init; }
        public required string ClientName { get; init; }
        public required string ClientSurname { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public string ClientPhNumber { get; init; } = null!;
        public List<GetOrderItems> Items { get; init; } = [];
        public OrderStatus Status { get; init; }
    }


}




