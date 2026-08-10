using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Persistence;


namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class GetCartEndpoint(ApplicationDbContext dbContext) : Endpoint<GetCartRequest, GetCartResponse, CartMapper>
    {

        public readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Get("api/customer/cart");
            Roles("customer");
            Validator<GetCartValidator>();

        }


        public override async Task HandleAsync(GetCartRequest req, CancellationToken ct)
        {
            var CartEntity = await _dbContext.CustomerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(r => r.CustomerId == req.UserId, ct);

            if (CartEntity is null)
            {
                await Send.NotFoundAsync();
                return;
            }

            var resp = Map.FromEntity(CartEntity);
            await Send.OkAsync(resp);
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

    public enum Gender { Male, Female }
    public sealed record GetCartResponse
    {
        
    }

}
