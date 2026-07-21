using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Administrator;

public class AdminConfig : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");
        builder.HasKey(a => a.Id);
        builder.HasOne(a => a.Profile)
               .WithOne(p => p.Admin)
               .HasForeignKey<AdminProfile>(p => p.AdminId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.HasMany(a => a.Products)
               .WithOne(pr => pr.Admin)
               .HasForeignKey(pr => pr.AdminId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

       

        builder.HasIndex(a => a.TgId).IsUnique(); 
        
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(a => a.TgId).HasColumnName("tgId");
    }
}