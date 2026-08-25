using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class AddItemToCart(ApplicationDbContext dbContext) : Endpoint<AddItemToCartRequest, AddItemToCartResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/customer/cart/items");
            Roles("customer");
            Validator<AddItemToCartValidator>();

        }


        public override async Task HandleAsync(AddItemToCartRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on a CATALOG page
            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == req.ProductId, ct);

            if (!productExists)
            {
                AddError("ProductId", "The specified product does not exist. Reload page to see what changed");
                await Send.ErrorsAsync();
                return;
            }
            // checks if the cart exists for the user, if not creates a new cart (1 of 2 possibilities)
            var bucket = await _dbContext.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == req.UserId, ct);

            if (bucket is null)
            {
                bucket = new Bucket
                {
                    Id = Guid.CreateVersion7(), // lol, my ValueGeneratedOnAdd working very nicely.
                    CustomerId = req.UserId,
                    

                    // we creating in db v7 guid, 
                };
                _dbContext.Carts.Add(bucket);
            }
            var existingCartItem = bucket.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
            if (existingCartItem == null) 
            {    
                var newCartItem = new BucketItem
                {
                    ProductId = req.ProductId,
                    BucketItemQuantity = 1,
                    BucketId = bucket.Id
                };
                _dbContext.CartItems.Add(newCartItem);

            }
            else
            {
                existingCartItem.BucketItemQuantity++;
            }
            // i hate this code but VSA AND REPR told me to do so,
            var productName = await _dbContext.Products.AsNoTracking().Where(i => i.Id == req.ProductId) 
                .Select(i => i.ProductName).FirstAsync(ct);
            
            await _dbContext.SaveChangesAsync(ct);
            
            await Send.OkAsync(new AddItemToCartResponse { Message = "Item added to cart successfully.", ProductName = productName  }); 





        }
    }

    public class AddItemToCartValidator : Validator<AddItemToCartRequest>
    {
        public AddItemToCartValidator()
        {
            RuleFor(x => x.UserId).NotNull().WithMessage("UserId is required.");
            RuleFor(x => x.ProductId).NotNull().WithMessage("ProductId is required.");
           
        }
    }



    public sealed record AddItemToCartRequest
    {
        [FromClaim]
        public Guid UserId { get; init; }
        public Guid ProductId { get; init; }
      
    }


    public sealed record AddItemToCartResponse  
    {
        public string ProductName { get; init; } = null!;
        public string Message { get; init; } = null!;

    }
    

    
}
