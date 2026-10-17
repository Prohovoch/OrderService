using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Persistence;



namespace OrderService.src.Deal.Customer
{
    public class AddAnItemToOrder(ApplicationDbContext dbContext) : Endpoint<AddAnItemToOrderRequest>
    {

        public override void Configure()
        {
            Post("api/customer/order");
            Roles("customer");
            Validator<AddAnItemToOrderValidator>();

        }


        public override async Task HandleAsync(AddAnItemToOrderRequest req, CancellationToken ct)
        {
            var entityId = await dbContext.Customers.Where(x => x.TgId == req.TelegramId).AsNoTracking().Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
            if (entityId is null)
            {
                AddError("CustomerId:", $"Customer with TelegramId {req.TelegramId} not found.");
                await Send.ForbiddenAsync();
                return;
            }
            var uniqueCartItemsIds = req.CartItemIds.ToHashSet();
            var selectedBucketItems = await dbContext.CartItems.Where(ci => uniqueCartItemsIds.Contains(ci.Id) && ci.Bucket!.CustomerId == entityId).ToListAsync(ct);

            if (selectedBucketItems.Count != uniqueCartItemsIds.Count)
            {
                AddError("Count: ", " Possible that it is not ur cart, or an item was deleted");
                await Send.ErrorsAsync();
                return;
            }


            var customerData = await dbContext.CustomerProfiles.Where(c => c.Id == entityId).AsNoTracking().Select(c => new { c.Name, c.Surname, c.PhoneNumber }).FirstOrDefaultAsync(ct);
            if(customerData is null)
            {
                AddError("CustomerProfile:", $"Customer profile with Id {entityId} not found.");
                await Send.NotFoundAsync();
                return;
            }   
            // take out products ids from selected  items;
            var productsIds = selectedBucketItems.Select(ci => ci.ProductId).ToHashSet();
            var products = await dbContext.Products.Where(p => productsIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct); // o(1)

            if (productsIds.Count != products.Count)
            {
                Logger.LogWarning("Some of the products were deleted from a catalog");
                AddError("ProductId:", $"An Item  was deleted");
                await Send.ErrorsAsync();
                return;
            }

            var order = new DomainOrder // Fast endpoint somehow have a defitnition for order???????
            {
                Id = Guid.CreateVersion7(),

                CustomerId = entityId,

                CustomerPhoneNumber = customerData.PhoneNumber,
                Status = OrderStatus.Created,
                CreatedAt = DateTimeOffset.UtcNow,
                ClientName = customerData.Name,
                ClientSurname = customerData.Surname,
                DisplayOrderNumber = (1000, 9999).ToString(), // remove
            };

            foreach (var item in selectedBucketItems)
            {
                var product = products[item.ProductId];
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
            Logger.LogInformation("Order created with {Count} items", order.Items.Count);
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync(ct);
            await Send.OkAsync(new OrderResponse { Id = order.Id });
        }
    }
    public class AddAnItemToOrderValidator : Validator<AddAnItemToOrderRequest>
    {
        public AddAnItemToOrderValidator()
        {
            RuleFor(x => x.TelegramId).NotEmpty().WithMessage("TelegramId is required!");
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
        [FromHeader("Customer-Telegram-Id")]
        public long TelegramId { get; init; }
        public required List<Guid> CartItemIds { get; init; }


    }
}




