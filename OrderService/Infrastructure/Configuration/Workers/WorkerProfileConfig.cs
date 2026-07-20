using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Infrastructure.Entities.Worker;
namespace OrderService.Infrastructure.Configuration.Workers
{
    public class WorkerProfileConfig : IEntityTypeConfiguration<WorkerProfile>
    {
        public void Configure(EntityTypeBuilder<WorkerProfile> builder)
        {
            builder.ToTable("worker_profiles");
            builder.HasKey(wp => wp.Id);

            builder.Property(wp => wp.Name).HasMaxLength(50).HasColumnName("name").IsRequired();
            builder.Property(wp => wp.Surname).HasMaxLength(50).HasColumnName("surname").IsRequired();
            builder.Property(wp => wp.Age).HasMaxLength(3).HasColumnName("age").IsRequired(false);
            builder.Property(wp => wp.Gender).HasMaxLength(10).HasColumnName("gender").IsRequired(false);
        }

    }
}
