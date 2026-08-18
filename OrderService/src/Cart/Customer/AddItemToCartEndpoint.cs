using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Cart;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Persistence;




namespace OrderService.src.Cart.Customer
{
    // REPR endpoint
    public class AddItemToCartEndpoint(ApplicationDbContext dbContext) : Endpoint<AddItemToCartRequest, AddItemToCartResponse>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/customer/cart/items");
            AllowAnonymous();
            Validator<AddItemToCartValidator>();

        }


        public override async Task HandleAsync(AddItemToCartRequest req, CancellationToken ct)
        {
            // checks if the product exists in the database on CATALOG page
            var productExists = await _dbContext.Products.AnyAsync(p => p.Id == req.ProductId, ct);

            if (!productExists)
            {
                AddError("ProductId", "The specified product does not exist. Reload page to see what changed");
                await Send.ErrorsAsync();
                return;
            }
            // checks if the cart exists for the user, if not creates a new cart (1 of 2 possibilities)
            var bucket = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CustomerId == req.UserId, ct);

            if (bucket is null)
            {
                var newCart = new Bucket
                {
                    Id = Guid.CreateVersion7(),
                    CustomerId = req.UserId,
                    
                   // we creating in db v7 guid, 
                };
                _dbContext.Carts.Add(newCart);
                

                bucket = newCart; //shut up this null warning, cause now we have a new cart and we can use it below 
            }
            // проверяет, есть ли уже товар в корзине, если есть, увеличивает количество на 1, если нет, добавляет новый товар в корзину 
            var existingCartItem = await _dbContext.CartItems.FirstOrDefaultAsync(i => i.ProductId == req.ProductId && i.BucketId == bucket.Id, ct);
            
            if (existingCartItem is not null)
            {
                existingCartItem.BucketItemQuantity++;
            }
            else
            {
                var newCartItem = new BucketItem
                {
                    ProductId = req.ProductId,
                    BucketItemQuantity = 1,
                    BucketId = bucket.Id
                };
                _dbContext.CartItems.Add(newCartItem);
                
            }

            var productName = await _dbContext.Products.AsNoTracking().Where(i => i.Id == req.ProductId)
                .Select(i => i.ProductName).FirstAsync(ct);
            
            await _dbContext.SaveChangesAsync(ct);
            
            await Send.OkAsync(new AddItemToCartResponse { Message = "Item added to cart successfully.", ProductName = productName  }); // hack!





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
      
        public Guid UserId { get; init; }
        public Guid ProductId { get; init; }
      
    }


    public sealed record AddItemToCartResponse  
    {
        public string ProductName { get; init; } = null!;
        public string Message { get; init; } = null!;

    }
    

    
}
