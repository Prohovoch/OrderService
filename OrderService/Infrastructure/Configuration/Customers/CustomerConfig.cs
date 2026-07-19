using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Customer;

namespace OrderService.Infrastructure.Configuration.Customers
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customer");
            builder.HasKey(c => c.Id);
            builder.HasOne(c => c.Profile)
                .WithOne(p => p.Customer)
                .HasForeignKey<CustomerProfile>(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);



        }
    }
}