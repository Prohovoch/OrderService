using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Administrator;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Entities.Cart;

using OrderService.Infrastructure.Entities.Employee;

namespace OrderService.Infrastructure.Persistence;
/// <summary>
/// Using this class we can access the database and perform CRUD operations on the entities defined in the application.
/// </summary>
/// <param name="options">DbContextParam for configs.</param>
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

    public DbSet<Bucket> Carts => Set<Bucket>();
    public DbSet<BucketItem> CartItems => Set<BucketItem>();

    // Catalog
    public DbSet<CatalogItem> Products => Set<CatalogItem>();
    // Orders
    public DbSet<DomainOrder> Orders => Set<DomainOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Creating our models and applying configurations from the assembly where ApplicationDbContext is defined.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
