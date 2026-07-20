using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Admin;
namespace OrderService.Infrastructure.Configuration.Admins
{
    public class AdminProfileConfig : IEntityTypeConfiguration<AdminProfile>
    {
        public void Configure(EntityTypeBuilder<AdminProfile> builder)
        {
            builder.ToTable("admin_profiles");
            builder.HasKey(ap => ap.Id);

            builder.Property(ap => ap.Id).HasColumnName("id").ValueGeneratedOnAdd();
            builder.Property(ap => ap.AdminId).HasColumnName("admin_id").IsRequired();

            builder.Property(ap => ap.Name).HasMaxLength(50).HasColumnName("name").IsRequired();
            builder.Property(ap => ap.Surname).HasMaxLength(50).HasColumnName("surname").IsRequired();
            builder.Property(ap => ap.Age).HasMaxLength(3).HasColumnName("age").IsRequired(false);
            builder.Property(ap => ap.Gender).HasMaxLength(10).HasColumnName("gender").IsRequired(false);
        }

    }
}
