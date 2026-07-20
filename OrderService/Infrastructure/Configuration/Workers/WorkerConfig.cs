using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Worker;


namespace OrderService.Infrastructure.Configuration.Workers
{
    public class WorkerConfig : IEntityTypeConfiguration<Worker>
    {
        public void Configure(EntityTypeBuilder<Worker> builder)
        {
            builder.ToTable("workers");
            builder.HasKey(w => w.Id);
            builder.HasOne(w => w.Profile)
                   .WithOne(p => p.Worker)
                   .HasForeignKey<WorkerProfile>(p => p.WorkerId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();
            builder.HasMany(w => w.Orders)
                   .WithOne(o => o.Worker)
                   .HasForeignKey(o => o.WorkerId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);
           
            builder.HasIndex(c => c.TgId).IsUnique();

            builder.Property(c => c.TgId).HasColumnName("tgId");
        }
    }
}

              
                  

       