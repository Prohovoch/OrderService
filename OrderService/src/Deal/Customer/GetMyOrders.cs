using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Customer
{
    public class CheckMyOrders(ApplicationDbContext dbContext) : Endpoint<GetMyOrdersRequest, GetMyOrdersResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/orders/me");
            Roles("customer");
            Validator<GetMyOrdersValidator>();

        }


        public override async Task HandleAsync(GetMyOrdersRequest req, CancellationToken ct)
        {
            // Get all orders which connects with the user.

            var userOrders = await _dbContext.Orders.AsNoTracking()
                .Where(o => o.CustomerId == req.UserId)
                .Select(o => new OrderResponseDto

                {
                    // first two params is checked for anon.
                    // CustomerName = o.Customer != null && o.Customer.Profile != null ? o.Customer.Profile.Name : "Неизвестно",
                    // CustomerSurname = o.Customer != null && o.Customer.Profile != null ? o.Customer.Profile.Surname : "Неизвестно",
                    
                    CustomerName = o.ClientName!, // Assuming o.Customer and o.Customer.Profile are not null and we know that user is active here.
                    CustomerSurname = o.ClientSurname!,
                    CreatedAt = o.CreatedAt,
                    CompletedAt = o.CompletedAt,


                    Items = o.Items.Select(oi => new GetOrderResponseItems
                    {
                        ProductName = oi.Details.ProductName,
                        Quantity = oi.Details.Quantity,
                        TotalPrice = oi.Details.TotalPrice,
                        Price = oi.Details.Price
                    }).ToList()
                })
                .ToListAsync(ct);

            await Send.OkAsync(new GetMyOrdersResponse { Orders = userOrders });


        }
    }

    public class GetMyOrdersValidator : Validator<GetMyOrdersRequest>
    {
        public GetMyOrdersValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId required!");
        }
    }



    public sealed record GetOrderResponseItems
    {

        public DateTimeOffset CreatedAt { get; init; }
        public string ProductName { get; init; } = null!;
        public int Quantity { get; init ; }
        public decimal TotalPrice { get; init; } // ?
        // price at purchase.
        public decimal Price { get; init; }
    }

    public sealed record GetMyOrdersResponse
    {

        public required List<OrderResponseDto> Orders{ get; init; }
       
    }

    public sealed record OrderResponseDto
    {
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public required string CustomerSurname { get; init; }
        public required string CustomerName { get; init; }
        public List<GetOrderResponseItems> Items { get; init; } = [];
    }
    public sealed record GetMyOrdersRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
    }
        
   

}

