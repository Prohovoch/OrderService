using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Deal;



namespace OrderService.Infrastructure.Configuration.Deal
{
    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");
            builder.HasKey(oi => oi.Id);
            builder.ComplexProperty(oi => oi.Details, oid => oid.ToJson());
            
                
            builder.Property(oi => oi.Id).HasColumnName("id").ValueGeneratedOnAdd();
            

        }
    }
}
