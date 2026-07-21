using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Draft;

namespace OrderService.Infrastructure.Configuration.Draft
{
    public class BucketConfig : IEntityTypeConfiguration<Bucket>
    {
        public void Configure(EntityTypeBuilder<Bucket> builder)
        {
            builder.ToTable("bucket");
            builder.HasKey(b => b.Id);
            builder.HasMany(b => b.Items)
                .WithOne(bi => bi.Bucket)
                .HasForeignKey(bi => bi.BucketId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedOnAdd();
      
        }
    }
}
