using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Draft;

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
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            builder.HasOne(c => c.Draft)
                .WithOne(d => d.Customer)
                .HasForeignKey<Bucket>(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);


            builder.HasIndex(c => c.TgId).IsUnique();

            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();






        }
    }
}