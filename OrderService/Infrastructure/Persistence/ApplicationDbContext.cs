using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Admin;
using OrderService.Infrastructure.Entities.Auth;
using OrderService.Infrastructure.Entities.Catalog;
using OrderService.Infrastructure.Entities.Buyer;
using OrderService.Infrastructure.Entities.Deal;
using OrderService.Infrastructure.Entities.Draft;

using OrderService.Infrastructure.Entities.Employee;

namespace OrderService.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    // users entities / models
    DbSet<Customer> Customers => Set<Customer>();
    DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    DbSet<Worker> Workers => Set<Worker>();
    DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    DbSet<Admin> Admins => Set<Admin>();
    DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();
    // Auth

    DbSet<Token> Tokens => Set<Token>();

    // Draft

    DbSet<Bucket> Drafts => Set<Bucket>();
    DbSet<BucketItem> DraftItems => Set<BucketItem>();

    // Catalog
    DbSet<CatalogItem> Products => Set<CatalogItem>();
    // Orders
    DbSet<Order> Orders => Set<Order>();
    DbSet<OrderItem> OrderItems => Set<OrderItem>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
