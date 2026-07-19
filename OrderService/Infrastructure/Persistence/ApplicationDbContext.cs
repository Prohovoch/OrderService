using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Entities.Admin;
using OrderService.Infrastructure.Entities.Customer;
using OrderService.Infrastructure.Entities.Worker;

namespace OrderService.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{

    // users entities / models
    DbSet<Customer> Customers => Set<Customer>();
    DbSet<CustomerProfile> CustomerProfile => Set<CustomerProfile>();
    DbSet<Worker> Workers => Set<Worker>();
    DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    DbSet<Admin> Admin => Set<Admin>();
    DbSet<AdminProfile> AdminProfile => Set<AdminProfile>();


}
