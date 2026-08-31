using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;
using OrderService.src.Cart.Customer;
using OrderService.src.Catalog.Admin;


namespace OrderService.src.Deal.Cutomer
{
    public class AddAnItemToOrder(ApplicationDbContext dbContext) : Endpoint<AddAnItemToOrderRequest>
    {

        private readonly ApplicationDbContext _dbContext = dbContext;

        public override void Configure()
        {
            Post("api/order");
            Roles("customer");
            Validator<AddAnItemToOrderValidator>();

        }


        public override async Task HandleAsync(AddAnItemToOrderRequest req, CancellationToken ct)
        {
            // 1. Check if that item have an available status.
            // 2. Check if the bucket thing is belongs to our request user.
            // 2. Put this objects into order item.
            var uniqueCartItemsIds = req.CartItemIds.Distinct().ToList();
            var selectedBucketItems = await _dbContext.CartItems.Where(ci => uniqueCartItemsIds.Contains(ci.Id) && ci.Bucket!.CustomerId == req.UserId).ToListAsync(ct);
            
            if(selectedBucketItems.Count != uniqueCartItemsIds.Count)
            {
                AddError("Count: ", " Possible that it is not ur cart, or an item was deleted");
                await Send.ErrorsAsync();
                return;
            }
            
            

            // take out products ids from selected  items;
            var productsIds = selectedBucketItems.Select(ci => ci.ProductId).Distinct().ToList();
            var products = await _dbContext.Products.Where(p => productsIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id); // o(1)

            if (productsIds.Count != products.Count)
            {
                AddError("ProductId:", "An Item was deleted");
                await Send.ErrorsAsync();
                return;
            }

            // Creating an Order object.

            var order = new DomainOrder // Fast endpoint somehow have a defitnition for order???????
            {
                Id = Guid.CreateVersion7(),
                CustomerId = req.UserId,
                Status = OrderStatus.Created
            };
        
        // welp no n+1...
            foreach (var item in selectedBucketItems)
            {
                var product = products[item.ProductId];
                if (product.AvailabilityStatus != ProductAvailabilityStatus.Available)
                {
                    AddError("Status: ", "This product cannot be assigned to order due avalaibility status.");
                    await Send.ErrorsAsync();
                    return;
                }

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Details = new OrderItemDetails
                    {

                        ProductName = product.ProductName,
                        Price = product.Price, //snap
                        Quantity = item.BucketItemQuantity
                        // description?
                    }
                });

            }
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(ct);
            await Send.OkAsync(new OrderResponse { Id = order.Id});
        }
    }
    public class AddAnItemToOrderValidator : Validator<AddAnItemToOrderRequest>
    {
        public AddAnItemToOrderValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId required!");
            RuleFor(x => x.CartItemIds).NotEmpty().WithMessage("An Empty order cannot be created!");
        }
    }

   

    public sealed record OrderResponse
    {
        public Guid Id { get; init; }
    }
    public sealed record AddAnItemToOrderRequest

    {
        // return a list of calatog items.
        // use a flattenned dto without heritance.
        [FromClaim]
        public Guid UserId { get; init; }
        public required List<Guid> CartItemIds { get; init; }

    }



}

