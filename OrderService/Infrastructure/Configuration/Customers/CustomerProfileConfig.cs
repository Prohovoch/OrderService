using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Buyer;
namespace OrderService.Infrastructure.Configuration.Customers
{
    public class CustomerProfileConfig : IEntityTypeConfiguration<CustomerProfile>
    {
        public void Configure(EntityTypeBuilder<CustomerProfile> builder)
        {
            builder.ToTable("customer_profiles");
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(cp => cp.CustomerId).HasColumnName("customer_id").IsRequired();

            builder.Property(cp => cp.Name).HasMaxLength(50).HasColumnName("name").IsRequired();
            builder.Property(cp => cp.Surname).HasMaxLength(50).HasColumnName("surname").IsRequired();
            builder.Property(cp => cp.Age).HasMaxLength(3).HasColumnName("age").IsRequired(false);
            builder.Property(cp => cp.Gender).HasMaxLength(10).HasColumnName("gender").IsRequired(false);
        }
    
    }
}
