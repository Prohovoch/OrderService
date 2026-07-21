using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Deal;


namespace OrderService.Infrastructure.Configuration.Deal
{
    public class BucketConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("order");
            builder.HasKey(o => o.Id);
            builder.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.ClientNoAction)
                .IsRequired();
            builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().IsRequired();

        }
    }
}
