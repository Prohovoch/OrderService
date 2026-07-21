using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Catalog;

namespace OrderService.Infrastructure.Configuration.Catalog
{
    public class CatalogConfig : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("catalog");
            builder.HasKey(c => c.Id);
            builder.HasMany(c => c.BucketItems)
                .WithOne(b => b.Product)
                .HasForeignKey(b => b.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.ComplexProperty(c => c.Details, d => d.ToJson());
            builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(50).HasColumnName("type");
            builder.Property(c => c.AvailabilityStatus).HasConversion<string>().HasMaxLength(50).HasColumnName("status");
            builder.Property(c => c.AdminId).HasColumnName("admin_id");
            builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(c => c.Quantity).HasColumnName("quantity");
            builder.Property(c => c.Description).HasMaxLength(500).HasColumnName("description");
            builder.Property(c => c.Price).HasColumnType("decimal(18,2)").HasColumnName("price");
        }
    }
}
