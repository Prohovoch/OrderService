using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Draft;

namespace OrderService.Infrastructure.Configuration.Draft
{
    public class BucketItemConfig : IEntityTypeConfiguration<BucketItem>
    {
        public void Configure(EntityTypeBuilder<BucketItem> builder)
        {
            builder.ToTable("bucket_item");
            builder.HasKey(bi => bi.Id);

            builder.Property(bi => bi.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(bi => bi.Quantity).HasColumnName("quantity").IsRequired();
        }
    }
}
