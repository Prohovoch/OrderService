using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Persistence;
using System.Diagnostics.Contracts;


namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class GetCartEndpoint(ApplicationDbContext dbContext) : Endpoint<GetCartRequest, GetCartResponse, CartMapper>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/cart");
            Roles("customer");
            Validator<GetCartValidator>();

        }


        public override async Task HandleAsync(GetCartRequest req, CancellationToken ct)
        {
            // TODO do this stuff.
        }
    }

    public class GetCartValidator : Validator<GetCartRequest>
    {
        public GetCartValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
        }
    }
    public class CartMapper : ResponseMapper<GetCartResponse, CustomerProfile>
    {
        public override GetCartResponse FromEntity(CustomerProfile e) => new()
        {
          
        };

    }

    public sealed record GetCartRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
    }

    
    public sealed record GetCartResponse
    {

        public List<CartItemResponse>? Items { get; init; } = [];
            
    }

    // We using flatenned version here due to the fact that we gonna have then 3 nested levels of objects in the response, and we want to avoid that for now.

    public enum GetCartItemAvailability { Available, OutOfStock, Discounted }

    public sealed record CartItemResponse
    {
        public string Naming { get; init; } = null!;
        public string Description { get; init; } = null!;
        public GetCartItemAvailability Availability { get; init; }
        public decimal Price { get; init; }

         

        
    }
}
