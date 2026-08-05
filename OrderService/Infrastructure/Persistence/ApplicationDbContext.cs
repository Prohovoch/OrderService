using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Entities.Cart;

using OrderService.Infrastructure.Entities.Employee;

namespace OrderService.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    // users entities / models
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
    // Draft

    public DbSet<Bucket> Drafts => Set<Bucket>();
    public DbSet<BucketItem> DraftItems => Set<BucketItem>();

    // Catalog
    public DbSet<CatalogItem> Products => Set<CatalogItem>();
    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
