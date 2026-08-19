using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class GetCartEndpoint(ApplicationDbContext dbContext) : Endpoint<GetCartRequest, GetCartResponse>
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
            // We need  a projection so we made this tyoe of query. Fast endpoints cant do any shit with custom dto from combo entity.
            var cartResponse = await _dbContext.Carts.Where(c => c.CustomerId == req.UserId).AsNoTracking()
                .Select(c => new GetCartResponse
                {
                    Items = c.Items.Select(i => new CartItemResponse
                    {
                        Name = i.Product.ProductName,
                        Description = i.Product.Description,
                        Availability = i.Product.AvailabilityStatus == 
                            ProductAvailabilityStatus.Available ?  GetCartItemAvailability.Available
                            : GetCartItemAvailability.OutOfStock, // oh shit i forgot about that dumpster fire.
                            
                            

                        
                        Ingredients = i.Product.Details.Ingredients,
                        Volume = i.Product.Details.Volume,
                        Weight = i.Product.Details.Weight,
                        Quantity = i.BucketItemQuantity, // idea - every time when we get cart, we can multiply 
                        Price = i.Product.Price * i.BucketItemQuantity,

                        // ???
                    }).ToList()

                }).FirstOrDefaultAsync(ct);

                // This shit is made by me :-)
                if (cartResponse == null)
                {
                    var bucket = new Bucket
                    {
                        Id = Guid.CreateVersion7(),
                        CustomerId = req.UserId
                    };
                    _dbContext.Carts.Add(bucket);
                    await _dbContext.SaveChangesAsync(ct);
                    
                    await Send.NoContentAsync();
                    return;
                }
              
                await Send.OkAsync(cartResponse);
                
            


               
        }
    }

    public class GetCartValidator : Validator<GetCartRequest>
    {
        public GetCartValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
        }
    }
    
    

    public sealed record GetCartRequest
    {

        public Guid UserId { get; init; }
    }

    
    public sealed record GetCartResponse
    {
        
        public List<CartItemResponse> Items { get; init; } = [];

    }

    // We using flatenned version here due to the fact that we gonna have then 3 nested levels of objects in the response, and we want to avoid that for now.

    public enum GetCartItemAvailability { Available, OutOfStock }

    public sealed record CartItemResponse
    {
        public string Name { get; init; } = null!;
        public string Description { get; init; } = null!;
        public GetCartItemAvailability Availability { get; init; }
        public decimal Price { get; init; }
        public int Quantity { get; init; }

        // from product details
        public List<string> Ingredients { get; set; } = [];
        public decimal? Volume { get; set; } // full vol.
        public decimal? Weight { get; set; } // full dish weight without any toppings. too much for a mvp :/


    }
}
